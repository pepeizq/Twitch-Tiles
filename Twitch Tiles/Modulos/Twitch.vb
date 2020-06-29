Imports Microsoft.Toolkit.Uwp.Helpers
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

    Public anchoColumna As Integer = 320
    Dim clave As String = "TwitchCarpeta"

    Public Async Sub Generar(boolBuscarCarpeta As Boolean)

        Dim helper As New LocalObjectStorageHelper

        Dim recursos As New Resources.ResourceLoader()

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim botonAñadir As Button = pagina.FindName("botonAñadirCarpetaTwitch")
        botonAñadir.IsEnabled = False

        Dim botonBorrar As Button = pagina.FindName("botonBorrarCarpetasTwitch")
        botonBorrar.IsEnabled = False

        Dim spProgreso As StackPanel = pagina.FindName("spProgreso")
        spProgreso.Visibility = Visibility.Visible

        Dim pbProgreso As ProgressBar = pagina.FindName("pbProgreso")
        pbProgreso.Value = 0

        Dim tbProgreso As TextBlock = pagina.FindName("tbProgreso")
        tbProgreso.Text = String.Empty

        Dim botonCache As Button = pagina.FindName("botonConfigLimpiarCache")
        botonCache.IsEnabled = False

        Dim gridSeleccionarJuego As Grid = pagina.FindName("gridSeleccionarJuego")
        gridSeleccionarJuego.Visibility = Visibility.Collapsed

        Dim gv As GridView = pagina.FindName("gvTiles")
        gv.Items.Clear()

        Dim listaJuegos As New List(Of Tile)

        If Await helper.FileExistsAsync("juegos") = True Then
            listaJuegos = Await helper.ReadFileAsync(Of List(Of Tile))("juegos")
        End If

        Dim tbCarpetas As TextBlock = pagina.FindName("tbCarpetasDetectadasTwitch")

        If Not tbCarpetas.Text = Nothing Then
            tbCarpetas.Text = String.Empty
        End If

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

                    Dim k As Integer = 0
                    For Each juego As TwitchDB In juegos
                        Dim añadir As Boolean = True
                        Dim g As Integer = 0
                        While g < listaJuegos.Count
                            If listaJuegos(g).ID = juego.ID Then
                                añadir = False
                            End If
                            g += 1
                        End While

                        If añadir = True Then
                            Dim imagen As String = String.Empty

                            Try
                                imagen = Await Cache.DescargarImagen(juego.Imagen, juego.ID, "base")
                            Catch ex As Exception

                            End Try

                            Dim tile As New Tile(juego.Titulo, juego.ID, "twitch://fuel-launch/" + juego.ID, imagen, imagen, imagen, imagen)

                            listaJuegos.Add(tile)
                        End If

                        pbProgreso.Value = CInt((100 / juegos.Count) * k)
                        tbProgreso.Text = k.ToString + "/" + juegos.Count.ToString
                        k += 1
                    Next
                End If
            End If
        End If

        Await helper.SaveFileAsync(Of List(Of Tile))("juegos", listaJuegos)

        spProgreso.Visibility = Visibility.Collapsed

        Dim gridTiles As Grid = pagina.FindName("gridTiles")
        Dim gridAvisoNoJuegos As Grid = pagina.FindName("gridAvisoNoJuegos")

        If Not listaJuegos Is Nothing Then
            If listaJuegos.Count > 0 Then
                gridTiles.Visibility = Visibility.Visible
                gridAvisoNoJuegos.Visibility = Visibility.Collapsed
                gridSeleccionarJuego.Visibility = Visibility.Visible

                listaJuegos.Sort(Function(x, y) x.Titulo.CompareTo(y.Titulo))

                gv.Items.Clear()

                For Each juego In listaJuegos
                    BotonEstilo(juego, gv)
                Next

                If boolBuscarCarpeta = True Then
                    Toast(listaJuegos.Count.ToString + " " + recursos.GetString("GamesDetected"), Nothing)
                End If
            Else
                gridTiles.Visibility = Visibility.Collapsed
                gridAvisoNoJuegos.Visibility = Visibility.Visible
                gridSeleccionarJuego.Visibility = Visibility.Collapsed
            End If
        Else
            gridTiles.Visibility = Visibility.Collapsed
            gridAvisoNoJuegos.Visibility = Visibility.Visible
            gridSeleccionarJuego.Visibility = Visibility.Collapsed
        End If

        botonAñadir.IsEnabled = True
        botonBorrar.IsEnabled = True
        botonCache.IsEnabled = True

    End Sub

    Public Sub BotonEstilo(juego As Tile, gv As GridView)

        Dim panel As New DropShadowPanel With {
            .Margin = New Thickness(5, 5, 5, 5),
            .ShadowOpacity = 0.9,
            .BlurRadius = 5,
            .MaxWidth = anchoColumna + 10
        }

        Dim boton As New Button

        Dim imagen As New ImageEx With {
            .Source = juego.ImagenGrande,
            .IsCacheEnabled = True,
            .Stretch = Stretch.UniformToFill,
            .Padding = New Thickness(0, 0, 0, 0)
        }

        boton.Tag = juego
        boton.Content = imagen
        boton.Padding = New Thickness(0, 0, 0, 0)
        boton.Background = New SolidColorBrush(Colors.Transparent)

        panel.Content = boton

        Dim tbToolTip As TextBlock = New TextBlock With {
            .Text = juego.Titulo,
            .FontSize = 16,
            .TextWrapping = TextWrapping.Wrap
        }

        ToolTipService.SetToolTip(boton, tbToolTip)
        ToolTipService.SetPlacement(boton, PlacementMode.Mouse)

        AddHandler boton.Click, AddressOf BotonTile_Click
        AddHandler boton.PointerEntered, AddressOf UsuarioEntraBoton
        AddHandler boton.PointerExited, AddressOf UsuarioSaleBoton

        gv.Items.Add(panel)

    End Sub

    Private Async Sub BotonTile_Click(sender As Object, e As RoutedEventArgs)

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim botonJuego As Button = e.OriginalSource
        Dim juego As Tile = botonJuego.Tag

        '---------------------------------------------

        Dim htmlSteam As String = Await Decompiladores.HttpClient(New Uri("https://store.steampowered.com/search/?term=" + juego.Titulo.Replace(" ", "+")))

        If Not htmlSteam = Nothing Then
            Dim temp5, temp6 As String
            Dim int5, int6 As Integer

            int5 = htmlSteam.IndexOf("<!-- List Items -->")

            If Not int5 = -1 Then
                temp5 = htmlSteam.Remove(0, int5)

                int5 = temp5.IndexOf("<span class=" + ChrW(34) + "title" + ChrW(34) + ">" + juego.Titulo + "</span>")

                If Not int5 = -1 Then
                    temp5 = temp5.Remove(int5, temp5.Length - int5)

                    int5 = temp5.LastIndexOf("data-ds-appid=")
                    temp5 = temp5.Remove(0, int5 + 15)

                    int6 = temp5.IndexOf(ChrW(34))
                    temp6 = temp5.Remove(int6, temp5.Length - int6)

                    Dim idSteam As String = temp6.Trim

                    juego.ImagenPequeña = Await Steam.SacarIcono(idSteam)
                    juego.ImagenAncha = "https://steamcdn-a.akamaihd.net/steam/apps/" + idSteam + "/header.jpg"
                End If
            End If
        End If

        '---------------------------------------------

        Dim botonAñadirTile As Button = pagina.FindName("botonAñadirTile")
        botonAñadirTile.Tag = juego

        Dim imagenJuegoSeleccionado As ImageEx = pagina.FindName("imagenJuegoSeleccionado")
        imagenJuegoSeleccionado.Source = New BitmapImage(New Uri(juego.ImagenAncha))

        Dim tbJuegoSeleccionado As TextBlock = pagina.FindName("tbJuegoSeleccionado")
        tbJuegoSeleccionado.Text = juego.Titulo

        Dim gridSeleccionarJuego As Grid = pagina.FindName("gridSeleccionarJuego")
        gridSeleccionarJuego.Visibility = Visibility.Collapsed

        Dim gvTiles As GridView = pagina.FindName("gvTiles")

        If gvTiles.ActualWidth > anchoColumna Then
            ApplicationData.Current.LocalSettings.Values("ancho_grid_tiles") = gvTiles.ActualWidth
        End If

        gvTiles.Width = anchoColumna
        gvTiles.Padding = New Thickness(0, 0, 15, 0)

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

        Dim imagenPequeña As ImageEx = pagina.FindName("imagenTilePequeña")
        imagenPequeña.Source = Nothing

        If Not juego.ImagenPequeña = Nothing Then
            imagenPequeña.Source = juego.ImagenPequeña
            imagenPequeña.Tag = juego.ImagenPequeña
        End If

        Dim imagenMediana As ImageEx = pagina.FindName("imagenTileMediana")
        imagenMediana.Source = Nothing

        Dim imagenAncha As ImageEx = pagina.FindName("imagenTileAncha")
        imagenAncha.Source = Nothing

        If Not juego.ImagenAncha = Nothing Then
            If Not juego.ImagenMediana = Nothing Then
                imagenMediana.Source = juego.ImagenMediana
                imagenMediana.Tag = juego.ImagenMediana
            Else
                imagenMediana.Source = juego.ImagenAncha
                imagenMediana.Tag = juego.ImagenAncha
            End If

            imagenAncha.Source = juego.ImagenAncha
            imagenAncha.Tag = juego.ImagenAncha
        End If

        Dim imagenGrande As ImageEx = pagina.FindName("imagenTileGrande")
        imagenGrande.Source = Nothing

        If Not juego.ImagenGrande = Nothing Then
            imagenGrande.Source = juego.ImagenGrande
            imagenGrande.Tag = juego.ImagenGrande
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

        Dim gv As GridView = pagina.FindName("gvTiles")
        gv.Items.Clear()

        Generar(False)

    End Sub

End Module
