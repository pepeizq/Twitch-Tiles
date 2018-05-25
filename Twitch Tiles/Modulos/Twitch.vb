Imports Microsoft.Toolkit.Uwp.UI.Animations
Imports Microsoft.Toolkit.Uwp.UI.Controls
Imports SQLite.Net
Imports SQLite.Net.Platform.WinRT
Imports Windows.Storage
Imports Windows.Storage.AccessCache
Imports Windows.Storage.Pickers
Imports Windows.UI
Imports Windows.UI.Core
Imports Windows.UI.Xaml.Media.Animation

Module Twitch

    Dim clave As String = "TwitchCarpeta"

    Public Async Sub Generar(boolBuscarCarpeta As Boolean)

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim botonAñadir As Button = pagina.FindName("botonAñadirCarpetaTwitch")
        botonAñadir.IsEnabled = False

        Dim botonBorrar As Button = pagina.FindName("botonBorrarCarpetasTwitch")
        botonBorrar.IsEnabled = False

        Dim pr As ProgressRing = pagina.FindName("prTilesTwitch")
        pr.Visibility = Visibility.Visible

        Dim gv As GridView = pagina.FindName("gridViewTilesTwitch")

        Dim tbCarpetas As TextBlock = pagina.FindName("tbCarpetasDetectadasTwitch")

        If Not tbCarpetas.Text = Nothing Then
            tbCarpetas.Text = ""
        End If

        Dim recursos As New Resources.ResourceLoader()
        Dim numCarpetas As ApplicationDataContainer = ApplicationData.Current.LocalSettings

        If boolBuscarCarpeta = True Then
            Try
                Dim picker As New FolderPicker()

                picker.FileTypeFilter.Add("*")
                picker.ViewMode = PickerViewMode.List

                Dim carpeta As StorageFolder = Await picker.PickSingleFolderAsync()
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(clave, carpeta)
                tbCarpetas.Text = carpeta.Path

            Catch ex As Exception

            End Try
        Else
            Dim carpeta As StorageFolder = Nothing

            Try
                carpeta = Await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(clave)
                tbCarpetas.Text = carpeta.Path
            Catch ex As Exception
                tbCarpetas.Text = ""
            End Try
        End If

        If tbCarpetas.Text = Nothing Then
            tbCarpetas.Text = recursos.GetString("Ninguna")
        Else
            tbCarpetas.Text = tbCarpetas.Text.Trim
        End If

        '-------------------------------------------------------------

        Dim listaFinal As List(Of Tile) = New List(Of Tile)

        Dim carpetaMaestra As StorageFolder = Nothing

        Try
            carpetaMaestra = Await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(clave)
        Catch ex As Exception

        End Try

        If Not carpetaMaestra Is Nothing Then
            Dim carpetaJuegos As StorageFolder = Await carpetaMaestra.GetFolderAsync("Games\Sql")

            If Not carpetaJuegos Is Nothing Then
                Dim fichero As String = carpetaJuegos.Path + "\GameProductInfo.sqlite"

                Dim bdOrigen As StorageFile = Await StorageFile.GetFileFromPathAsync(fichero)
                Dim bdFinal As StorageFile = Nothing

                Try
                    bdFinal = Await ApplicationData.Current.LocalFolder.CreateFileAsync("basedatos.sqlite", CreationCollisionOption.ReplaceExisting)
                Catch ex As Exception

                End Try

                If Not bdFinal Is Nothing Then
                    Await bdOrigen.CopyAndReplaceAsync(bdFinal)

                    Dim conexion As New SQLiteConnection(New SQLitePlatformWinRT, bdFinal.Path, Interop.SQLiteOpenFlags.ReadWrite)

                    Dim juegos As TableQuery(Of TwitchDB) = conexion.Table(Of TwitchDB)

                    For Each juego As TwitchDB In juegos
                        Dim tile As New Tile(juego.Titulo, juego.Id, "twitch://fuel-launch/" + juego.Id, Nothing, New Uri(juego.Imagen), Nothing, New Uri(juego.Imagen))

                        listaFinal.Add(tile)
                    Next
                End If
            End If
        End If

        Dim panelAvisoNoJuegos As Grid = pagina.FindName("panelAvisoNoJuegos")
        Dim gridSeleccionar As Grid = pagina.FindName("gridSeleccionarJuego")

        If listaFinal.Count > 0 Then
            panelAvisoNoJuegos.Visibility = Visibility.Collapsed
            gridSeleccionar.Visibility = Visibility.Visible

            listaFinal.Sort(Function(x, y) x.Titulo.CompareTo(y.Titulo))

            gv.Items.Clear()

            For Each juego In listaFinal
                Dim boton As New Button

                Dim imagen As New ImageEx

                Try
                    imagen.Source = New BitmapImage(juego.ImagenGrande)
                Catch ex As Exception

                End Try

                imagen.IsCacheEnabled = True
                imagen.Stretch = Stretch.Uniform
                imagen.Padding = New Thickness(0, 0, 0, 0)

                boton.Tag = juego
                boton.Content = imagen
                boton.Padding = New Thickness(0, 0, 0, 0)
                boton.BorderThickness = New Thickness(1, 1, 1, 1)
                boton.BorderBrush = New SolidColorBrush(Colors.Black)
                boton.Background = New SolidColorBrush(Colors.Transparent)

                Dim tbToolTip As TextBlock = New TextBlock With {
                    .Text = juego.Titulo,
                    .FontSize = 16
                }

                ToolTipService.SetToolTip(boton, tbToolTip)
                ToolTipService.SetPlacement(boton, PlacementMode.Mouse)

                AddHandler boton.Click, AddressOf BotonTile_Click
                AddHandler boton.PointerEntered, AddressOf UsuarioEntraBoton
                AddHandler boton.PointerExited, AddressOf UsuarioSaleBoton

                gv.Items.Add(boton)
            Next

            If boolBuscarCarpeta = True Then
                Toast(listaFinal.Count.ToString + " " + recursos.GetString("GamesDetected"), Nothing)
            End If
        Else
            panelAvisoNoJuegos.Visibility = Visibility.Visible
            gridSeleccionar.Visibility = Visibility.Collapsed
        End If

        botonAñadir.IsEnabled = True
        botonBorrar.IsEnabled = True
        pr.Visibility = Visibility.Collapsed

    End Sub

    Private Async Sub BotonTile_Click(sender As Object, e As RoutedEventArgs)

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim botonJuego As Button = e.OriginalSource
        Dim juego As Tile = botonJuego.Tag

        Dim botonAñadirTile As Button = pagina.FindName("botonAñadirTile")
        botonAñadirTile.Tag = juego

        Dim imagenJuegoSeleccionado As ImageEx = pagina.FindName("imagenJuegoSeleccionado")
        imagenJuegoSeleccionado.Source = New BitmapImage(juego.ImagenGrande)

        Dim tbJuegoSeleccionado As TextBlock = pagina.FindName("tbJuegoSeleccionado")
        tbJuegoSeleccionado.Text = juego.Titulo

        Dim gridAñadir As Grid = pagina.FindName("gridAñadirTile")
        gridAñadir.Visibility = Visibility.Visible

        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("tile", botonJuego)

        Dim animacion As ConnectedAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("tile")

        If Not animacion Is Nothing Then
            animacion.TryStart(gridAñadir)
        End If

        Dim tbTitulo As TextBlock = pagina.FindName("tbTitulo")
        tbTitulo.Text = Package.Current.DisplayName + " (" + Package.Current.Id.Version.Major.ToString + "." + Package.Current.Id.Version.Minor.ToString + "." + Package.Current.Id.Version.Build.ToString + "." + Package.Current.Id.Version.Revision.ToString + ") - " + juego.Titulo

        '---------------------------------------------

        Dim htmlSteam As String = Await Decompiladores.HttpClient(New Uri("https://store.steampowered.com/search/?term=" + juego.Titulo.Replace(" ", "+")))

        If Not htmlSteam = Nothing Then
            Dim temp5, temp6 As String
            Dim int5, int6 As Integer

            int5 = htmlSteam.IndexOf("<!-- List Items -->")
            temp5 = htmlSteam.Remove(0, int5)

            int5 = temp5.IndexOf("<span class=" + ChrW(34) + "title" + ChrW(34) + ">" + juego.Titulo + "</span>")
            temp5 = temp5.Remove(int5, temp5.Length - int5)

            int5 = temp5.LastIndexOf("data-ds-appid=")
            temp5 = temp5.Remove(0, int5 + 15)

            int6 = temp5.IndexOf(ChrW(34))
            temp6 = temp5.Remove(int6, temp5.Length - int6)

            Dim idSteam As String = temp6.Trim

            juego.ImagenPequeña = Await SacarIcono(idSteam)
            juego.ImagenAncha = New Uri("http://cdn.edgecast.steamstatic.com/steam/apps/" + idSteam + "/header.jpg", UriKind.RelativeOrAbsolute)
        End If

        '---------------------------------------------

        Dim titulo1 As TextBlock = pagina.FindName("tituloTileAnchaEnseñar")
        Dim titulo2 As TextBlock = pagina.FindName("tituloTileAnchaPersonalizar")

        Dim titulo3 As TextBlock = pagina.FindName("tituloTileGrandeEnseñar")
        Dim titulo4 As TextBlock = pagina.FindName("tituloTileGrandePersonalizar")

        titulo1.Text = juego.Titulo
        titulo2.Text = juego.Titulo

        titulo3.Text = juego.Titulo
        titulo4.Text = juego.Titulo

        If Not juego.ImagenPequeña = Nothing Then
            Dim imagenPequeña1 As ImageEx = pagina.FindName("imagenTilePequeñaEnseñar")
            Dim imagenPequeña2 As ImageEx = pagina.FindName("imagenTilePequeñaGenerar")
            Dim imagenPequeña3 As ImageEx = pagina.FindName("imagenTilePequeñaPersonalizar")

            imagenPequeña1.Source = juego.ImagenPequeña
            imagenPequeña2.Source = juego.ImagenPequeña
            imagenPequeña3.Source = juego.ImagenPequeña

            imagenPequeña1.Tag = juego.ImagenPequeña
            imagenPequeña2.Tag = juego.ImagenPequeña
            imagenPequeña3.Tag = juego.ImagenPequeña
        End If

        If Not juego.ImagenMediana = Nothing Then
            Dim imagenMediana1 As ImageEx = pagina.FindName("imagenTileMedianaEnseñar")
            Dim imagenMediana2 As ImageEx = pagina.FindName("imagenTileMedianaGenerar")
            Dim imagenMediana3 As ImageEx = pagina.FindName("imagenTileMedianaPersonalizar")

            imagenMediana1.Source = juego.ImagenMediana
            imagenMediana2.Source = juego.ImagenMediana
            imagenMediana3.Source = juego.ImagenMediana

            imagenMediana1.Tag = juego.ImagenMediana
            imagenMediana2.Tag = juego.ImagenMediana
            imagenMediana3.Tag = juego.ImagenMediana
        End If

        If Not juego.ImagenAncha = Nothing Then
            Dim imagenAncha1 As ImageEx = pagina.FindName("imagenTileAnchaEnseñar")
            Dim imagenAncha2 As ImageEx = pagina.FindName("imagenTileAnchaGenerar")
            Dim imagenAncha3 As ImageEx = pagina.FindName("imagenTileAnchaPersonalizar")

            imagenAncha1.Source = juego.ImagenAncha
            imagenAncha2.Source = juego.ImagenAncha
            imagenAncha3.Source = juego.ImagenAncha

            imagenAncha1.Tag = juego.ImagenAncha
            imagenAncha2.Tag = juego.ImagenAncha
            imagenAncha3.Tag = juego.ImagenAncha
        End If

        If Not juego.ImagenGrande = Nothing Then
            Dim imagenGrande1 As ImageEx = pagina.FindName("imagenTileGrandeEnseñar")
            Dim imagenGrande2 As ImageEx = pagina.FindName("imagenTileGrandeGenerar")
            Dim imagenGrande3 As ImageEx = pagina.FindName("imagenTileGrandePersonalizar")

            imagenGrande1.Source = juego.ImagenGrande
            imagenGrande2.Source = juego.ImagenGrande
            imagenGrande3.Source = juego.ImagenGrande

            imagenGrande1.Tag = juego.ImagenGrande
            imagenGrande2.Tag = juego.ImagenGrande
            imagenGrande3.Tag = juego.ImagenGrande
        End If

    End Sub

    Private Sub UsuarioEntraBoton(sender As Object, e As PointerRoutedEventArgs)

        Dim boton As Button = sender
        Dim imagen As ImageEx = boton.Content

        imagen.Saturation(0).Start()

        Window.Current.CoreWindow.PointerCursor = New CoreCursor(CoreCursorType.Hand, 1)

    End Sub

    Private Sub UsuarioSaleBoton(sender As Object, e As PointerRoutedEventArgs)

        Dim boton As Button = sender
        Dim imagen As ImageEx = boton.Content

        imagen.Saturation(1).Start()

        Window.Current.CoreWindow.PointerCursor = New CoreCursor(CoreCursorType.Arrow, 1)

    End Sub

    Public Sub Borrar()

        StorageApplicationPermissions.FutureAccessList.Clear()

        Dim recursos As New Resources.ResourceLoader()

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content
        Dim tbCarpetas As TextBlock = pagina.FindName("tbCarpetasDetectadasTwitch")

        tbCarpetas.Text = recursos.GetString("Ninguna")

        Dim gv As GridView = pagina.FindName("gridViewTilesTwitch")
        gv.Items.Clear()

        Generar(False)

    End Sub

    Public Async Function SacarIcono(id As String) As Task(Of Uri)

        Dim html As String = Await Decompiladores.HttpClient(New Uri("https://store.steampowered.com/app/" + id + "/"))
        Dim uriIcono As Uri = Nothing

        If Not html = Nothing Then
            If html.Contains("<div class=" + ChrW(34) + "apphub_AppIcon") Then
                Dim temp, temp2 As String
                Dim int, int2 As Integer

                int = html.IndexOf("<div class=" + ChrW(34) + "apphub_AppIcon")
                temp = html.Remove(0, int)

                int = temp.IndexOf("<img src=")
                temp = temp.Remove(0, int + 10)

                int2 = temp.IndexOf(ChrW(34))
                temp2 = temp.Remove(int2, temp.Length - int2)

                temp2 = temp2.Replace("%CDN_HOST_MEDIA_SSL%", "steamcdn-a.akamaihd.net")

                uriIcono = New Uri(temp2.Trim)
            End If
        End If

        If uriIcono = Nothing Then
            html = Await Decompiladores.HttpClient(New Uri("https://steamdb.info/app/" + id + "/"))

            If Not html = Nothing Then
                If html.Contains("<img class=" + ChrW(34) + "app-icon avatar") Then
                    Dim temp, temp2 As String
                    Dim int, int2 As Integer

                    int = html.IndexOf("<img class=" + ChrW(34) + "app-icon avatar")
                    temp = html.Remove(0, int)

                    int = temp.IndexOf("src=")
                    temp = temp.Remove(0, int + 5)

                    int2 = temp.IndexOf(ChrW(34))
                    temp2 = temp.Remove(int2, temp.Length - int2)

                    uriIcono = New Uri(temp2.Trim)
                End If
            End If
        End If

        Return uriIcono
    End Function

End Module
