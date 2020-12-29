Imports Microsoft.Toolkit.Uwp.Helpers
Imports Microsoft.Toolkit.Uwp.UI.Controls
Imports SQLite.Net
Imports SQLite.Net.Platform.WinRT
Imports Windows.Storage
Imports Windows.Storage.AccessCache
Imports Windows.Storage.Pickers
Imports Windows.UI
Imports Windows.UI.Xaml.Media.Animation

Module Twitch

    Public anchoColumna As Integer = 180
    Dim clave As String = "TwitchFichero2"

    Public Async Sub Generar(buscarFichero As Boolean)

        Dim helper As New LocalObjectStorageHelper

        Dim recursos As New Resources.ResourceLoader()

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Configuracion.Estado(False)
        Cache.Estado(False)

        Dim gv As AdaptiveGridView = pagina.FindName("gvTiles")
        gv.DesiredWidth = anchoColumna
        gv.Items.Clear()

        Dim listaJuegos As New List(Of Tile)

        If Await helper.FileExistsAsync("juegos") = True Then
            listaJuegos = Await helper.ReadFileAsync(Of List(Of Tile))("juegos")
        End If

        If listaJuegos Is Nothing Then
            listaJuegos = New List(Of Tile)
        End If

        If buscarFichero = True Then
            Try
                Dim picker As New FileOpenPicker()
                picker.FileTypeFilter.Add(".sqlite")
                picker.ViewMode = PickerViewMode.List

                Dim fichero As StorageFile = Await picker.PickSingleFileAsync

                If Not fichero Is Nothing Then
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(clave, fichero)
                End If
            Catch ex As Exception

            End Try
        End If

        '-------------------------------------------------------------

        Dim ficheroMaestro As StorageFile = Nothing

        Try
            ficheroMaestro = Await StorageApplicationPermissions.FutureAccessList.GetFileAsync(clave)
        Catch ex As Exception

        End Try

        If Not ficheroMaestro Is Nothing Then
            Dim gridProgreso As Grid = pagina.FindName("gridProgreso")
            Interfaz.Pestañas.Visibilidad(gridProgreso, Nothing, Nothing)

            Dim pbProgreso As ProgressBar = pagina.FindName("pbProgreso")
            pbProgreso.Value = 0

            Dim tbProgreso As TextBlock = pagina.FindName("tbProgreso")
            tbProgreso.Text = String.Empty

            Dim bdFinal As StorageFile = Nothing

            Try
                bdFinal = Await ApplicationData.Current.LocalFolder.CreateFileAsync("basedatos.sqlite", CreationCollisionOption.ReplaceExisting)
            Catch ex As Exception

            End Try

            If Not bdFinal Is Nothing Then
                Await ficheroMaestro.CopyAndReplaceAsync(bdFinal)

                Dim conexion As New SQLiteConnection(New SQLitePlatformWinRT, bdFinal.Path, Interop.SQLiteOpenFlags.ReadOnly)

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

        Try
            Await helper.SaveFileAsync(Of List(Of Tile))("juegos", listaJuegos)
        Catch ex As Exception

        End Try

        Dim iconoResultado As FontAwesome5.FontAwesome = pagina.FindName("iconoResultado")

        If Not listaJuegos Is Nothing Then
            If listaJuegos.Count > 0 Then
                Dim gridJuegos As Grid = pagina.FindName("gridJuegos")
                Interfaz.Pestañas.Visibilidad(gridJuegos, recursos.GetString("Games"), Nothing)
                iconoResultado.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Check

                listaJuegos.Sort(Function(x, y) x.Titulo.CompareTo(y.Titulo))

                gv.Items.Clear()

                For Each juego In listaJuegos
                    BotonEstilo(juego, gv)
                Next
            Else
                Dim gridAvisoNoJuegos As Grid = pagina.FindName("gridAvisoNoJuegos")
                Interfaz.Pestañas.Visibilidad(gridAvisoNoJuegos, Nothing, Nothing)
                iconoResultado.Icon = Nothing
            End If
        Else
            Dim gridAvisoNoJuegos As Grid = pagina.FindName("gridAvisoNoJuegos")
            Interfaz.Pestañas.Visibilidad(gridAvisoNoJuegos, Nothing, Nothing)
            iconoResultado.Icon = Nothing
        End If

        Configuracion.Estado(True)
        Cache.Estado(True)

    End Sub

    Public Sub BotonEstilo(juego As Tile, gv As GridView)

        Dim panel As New DropShadowPanel With {
            .Margin = New Thickness(10, 10, 10, 10),
            .ShadowOpacity = 0.9,
            .BlurRadius = 10,
            .MaxWidth = anchoColumna + 20,
            .HorizontalAlignment = HorizontalAlignment.Center,
            .VerticalAlignment = VerticalAlignment.Center
        }

        Dim boton As New Button

        Dim imagen As New ImageEx With {
            .Source = juego.ImagenGrande,
            .IsCacheEnabled = True,
            .Stretch = Stretch.UniformToFill,
            .Padding = New Thickness(0, 0, 0, 0),
            .HorizontalAlignment = HorizontalAlignment.Center,
            .VerticalAlignment = VerticalAlignment.Center,
            .EnableLazyLoading = True
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
        AddHandler boton.PointerEntered, AddressOf Interfaz.Entra_Boton_Imagen
        AddHandler boton.PointerExited, AddressOf Interfaz.Sale_Boton_Imagen

        gv.Items.Add(panel)

    End Sub

    Private Async Sub BotonTile_Click(sender As Object, e As RoutedEventArgs)

        Trial.Detectar()
        Interfaz.AñadirTile.ResetearValores()

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim botonJuego As Button = e.OriginalSource
        Dim juego As Tile = botonJuego.Tag

        Dim gridAñadirTile As Grid = pagina.FindName("gridAñadirTile")
        Interfaz.Pestañas.Visibilidad(gridAñadirTile, juego.Titulo, Nothing)

        '---------------------------------------------

        ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("animacionJuego", botonJuego)
        Dim animacion As ConnectedAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("animacionJuego")

        If Not animacion Is Nothing Then
            animacion.Configuration = New BasicConnectedAnimationConfiguration
            animacion.TryStart(gridAñadirTile)
        End If

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

        '---------------------------------------------

        Dim imagenPequeña As ImageEx = pagina.FindName("imagenTilePequeña")
        imagenPequeña.Source = Nothing

        Dim imagenMediana As ImageEx = pagina.FindName("imagenTileMediana")
        imagenMediana.Source = Nothing

        Dim imagenAncha As ImageEx = pagina.FindName("imagenTileAncha")
        imagenAncha.Source = Nothing

        Dim imagenGrande As ImageEx = pagina.FindName("imagenTileGrande")
        imagenGrande.Source = Nothing

        If Not juego.ImagenPequeña = Nothing Then
            imagenPequeña.Source = juego.ImagenPequeña
            imagenPequeña.Tag = juego.ImagenPequeña
        End If

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

        If Not juego.ImagenGrande = Nothing Then
            imagenGrande.Source = juego.ImagenGrande
            imagenGrande.Tag = juego.ImagenGrande
        End If

    End Sub

End Module
