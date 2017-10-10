Imports Microsoft.Toolkit.Uwp.UI.Controls
Imports Windows.Storage
Imports Windows.Storage.AccessCache
Imports Windows.Storage.Pickers
Imports Windows.Storage.Streams
Imports Windows.UI
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

        Dim recursos As Resources.ResourceLoader = New Resources.ResourceLoader()
        Dim numCarpetas As ApplicationDataContainer = ApplicationData.Current.LocalSettings

        If boolBuscarCarpeta = True Then
            Try
                Dim picker As FolderPicker = New FolderPicker()

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
            Dim carpetaFuel As StorageFolder = Await carpetaMaestra.GetFolderAsync("Fuel\db\GameProductInfo")

            If Not carpetaFuel Is Nothing Then
                Dim ficheros As IReadOnlyList(Of StorageFile) = Await carpetaFuel.GetFilesAsync()

                For Each fichero In ficheros
                    If fichero.FileType.Contains(".cfs") Then
                        Dim texto As String = Nothing

                        Using inputStream As IRandomAccessStreamWithContentType = Await fichero.OpenReadAsync
                            Using clasicoStream As Stream = inputStream.AsStreamForRead
                                Using sr As StreamReader = New StreamReader(clasicoStream)
                                    While sr.Peek <> 0
                                        texto = texto + sr.ReadLine
                                    End While
                                End Using
                            End Using
                        End Using

                        If Not texto = Nothing Then
                            Dim i As Integer = 0
                            While i < 200
                                If texto.Contains(ChrW(34) + "ProductTitle" + ChrW(34)) Then
                                    Dim temp, temp2 As String
                                    Dim int, int2 As Integer

                                    int = texto.IndexOf(ChrW(34) + "ProductTitle" + ChrW(34))
                                    temp = texto.Remove(0, int + 14)

                                    texto = temp

                                    int = temp.IndexOf(ChrW(34))
                                    temp = temp.Remove(0, int + 1)

                                    int2 = temp.IndexOf(ChrW(34))
                                    temp2 = temp.Remove(int2, temp.Length - int2)

                                    Dim titulo As String = temp2.Trim

                                    Dim temp3, temp4 As String
                                    Dim int3, int4 As Integer

                                    int3 = texto.IndexOf(ChrW(34) + "Id" + ChrW(34))
                                    temp3 = texto.Remove(0, int3 + 4)

                                    int3 = temp3.IndexOf(ChrW(34))
                                    temp3 = temp3.Remove(0, int3 + 1)

                                    int4 = temp3.IndexOf(ChrW(34))
                                    temp4 = temp3.Remove(int4, temp3.Length - int4)

                                    Dim id As String = temp4.Trim

                                    Dim tituloBool As Boolean = False
                                    Dim g As Integer = 0
                                    While g < listaFinal.Count
                                        If listaFinal(g).Titulo = titulo Then
                                            tituloBool = True
                                        End If
                                        g += 1
                                    End While

                                    If tituloBool = False Then
                                        Dim htmlSteam As String = Await Decompiladores.HttpClient(New Uri("http://store.steampowered.com/search/?term=" + titulo.Replace(" ", "+")))

                                        If Not htmlSteam = Nothing Then
                                            Dim temp5, temp6 As String
                                            Dim int5, int6 As Integer

                                            int5 = htmlSteam.IndexOf("<!-- List Items -->")
                                            temp5 = htmlSteam.Remove(0, int5)

                                            int5 = temp5.IndexOf("<span class=" + ChrW(34) + "title" + ChrW(34) + ">" + titulo + "</span>")
                                            temp5 = temp5.Remove(int5, temp5.Length - int5)

                                            int5 = temp5.LastIndexOf("<img src=")
                                            temp5 = temp5.Remove(0, int5 + 10)

                                            int6 = temp5.IndexOf(ChrW(34))
                                            temp6 = temp5.Remove(int6, temp5.Length - int6)

                                            temp6 = temp6.Replace("capsule_sm_120", "header")

                                            Dim imagen As Uri = New Uri(temp6.Trim)

                                            Dim juego As New Tile(titulo, id, New Uri("twitch://fuel-launch/" + id), imagen, "Twitch", Nothing)
                                            juego.Tile = juego

                                            listaFinal.Add(juego)
                                        End If
                                    End If
                                End If
                                i += 1
                            End While
                        End If
                    End If
                Next
            End If
        End If

        Dim panelAvisoNoJuegos As DropShadowPanel = pagina.FindName("panelAvisoNoJuegos")
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
                    imagen.Source = New BitmapImage(juego.Imagen)
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

    Private Sub BotonTile_Click(sender As Object, e As RoutedEventArgs)

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim tbTitulo As TextBlock = pagina.FindName("tbTitulo")

        Dim gv As GridView = pagina.FindName("gridViewTilesTwitch")

        Dim botonJuego As Button = e.OriginalSource

        Dim borde As Thickness = New Thickness(6, 6, 6, 6)
        If botonJuego.BorderThickness = borde Then
            botonJuego.BorderThickness = New Thickness(1, 1, 1, 1)
            botonJuego.BorderBrush = New SolidColorBrush(Colors.Black)

            Dim gridAñadir As Grid = pagina.FindName("gridAñadirTiles")
            gridAñadir.Visibility = Visibility.Collapsed

            Dim gridSeleccionar As Grid = pagina.FindName("gridSeleccionarJuego")
            gridSeleccionar.Visibility = Visibility.Visible

            Dim recursos As New Resources.ResourceLoader()
            tbTitulo.Text = Package.Current.DisplayName + " (" + Package.Current.Id.Version.Major.ToString + "." + Package.Current.Id.Version.Minor.ToString + "." + Package.Current.Id.Version.Build.ToString + "." + Package.Current.Id.Version.Revision.ToString + ") - " + recursos.GetString("Tiles")
        Else
            For Each item In gv.Items
                Dim itemBoton As Button = item
                itemBoton.BorderThickness = New Thickness(1, 1, 1, 1)
                itemBoton.BorderBrush = New SolidColorBrush(Colors.Black)
            Next

            botonJuego.BorderThickness = New Thickness(6, 6, 6, 6)
            botonJuego.BorderBrush = New SolidColorBrush(App.Current.Resources("ColorSecundario"))

            Dim botonAñadirTile As Button = pagina.FindName("botonAñadirTile")
            Dim juego As Tile = botonJuego.Tag
            botonAñadirTile.Tag = juego

            Dim imageJuegoSeleccionado As ImageEx = pagina.FindName("imageJuegoSeleccionado")
            Dim imagenCapsula As String = juego.Imagen.ToString

            imageJuegoSeleccionado.Source = New BitmapImage(New Uri(imagenCapsula))

            Dim tbJuegoSeleccionado As TextBlock = pagina.FindName("tbJuegoSeleccionado")
            tbJuegoSeleccionado.Text = juego.Titulo

            Dim gridAñadir As Grid = pagina.FindName("gridAñadirTiles")
            gridAñadir.Visibility = Visibility.Visible

            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("tile", botonJuego)

            Dim animacion As ConnectedAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("tile")

            If Not animacion Is Nothing Then
                animacion.TryStart(gridAñadir)
            End If

            Dim gridSeleccionar As Grid = pagina.FindName("gridSeleccionarJuego")
            gridSeleccionar.Visibility = Visibility.Collapsed

            tbTitulo.Text = Package.Current.DisplayName + " (" + Package.Current.Id.Version.Major.ToString + "." + Package.Current.Id.Version.Minor.ToString + "." + Package.Current.Id.Version.Build.ToString + "." + Package.Current.Id.Version.Revision.ToString + ") - " + juego.Titulo
        End If

    End Sub

    Public Sub Borrar()

        StorageApplicationPermissions.FutureAccessList.Clear()

        Dim recursos As Resources.ResourceLoader = New Resources.ResourceLoader()

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content
        Dim tbCarpetas As TextBlock = pagina.FindName("tbCarpetasDetectadasTwitch")

        tbCarpetas.Text = recursos.GetString("Ninguna")

        Dim gv As GridView = pagina.FindName("gridViewTilesTwitch")
        gv.Items.Clear()

        Generar(False)

    End Sub

End Module
