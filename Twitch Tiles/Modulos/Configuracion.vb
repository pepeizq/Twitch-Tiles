Module Configuracion

    Public Sub Cargar()

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim botonAbrirConfig As Button = pagina.FindName("botonAbrirConfig")

        AddHandler botonAbrirConfig.Click, AddressOf AbrirConfigClick
        AddHandler botonAbrirConfig.PointerEntered, AddressOf Interfaz.EfectosHover.Entra_Boton_IconoTexto
        AddHandler botonAbrirConfig.PointerExited, AddressOf Interfaz.EfectosHover.Sale_Boton_IconoTexto

        Dim botonConfigImagen1 As Button = pagina.FindName("botonConfigImagen1")

        AddHandler botonConfigImagen1.Click, AddressOf AbrirImagen1Click
        AddHandler botonConfigImagen1.PointerEntered, AddressOf Interfaz.EfectosHover.Entra_Boton_IconoTexto
        AddHandler botonConfigImagen1.PointerExited, AddressOf Interfaz.EfectosHover.Sale_Boton_IconoTexto

        Dim botonConfigImagen2 As Button = pagina.FindName("botonConfigImagen2")

        AddHandler botonConfigImagen2.Click, AddressOf AbrirImagen2Click
        AddHandler botonConfigImagen2.PointerEntered, AddressOf Interfaz.EfectosHover.Entra_Boton_IconoTexto
        AddHandler botonConfigImagen2.PointerExited, AddressOf Interfaz.EfectosHover.Sale_Boton_IconoTexto

        Dim botonBuscarCarpetaTwitch As Button = pagina.FindName("botonBuscarCarpetaTwitch")

        AddHandler botonBuscarCarpetaTwitch.Click, AddressOf BuscarTwitchClick
        AddHandler botonBuscarCarpetaTwitch.PointerEntered, AddressOf Interfaz.EfectosHover.Entra_Boton_IconoTexto
        AddHandler botonBuscarCarpetaTwitch.PointerExited, AddressOf Interfaz.EfectosHover.Sale_Boton_IconoTexto

    End Sub

    Private Sub AbrirConfigClick(sender As Object, e As RoutedEventArgs)

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim gridConfig As Grid = pagina.FindName("gridConfig")

        Dim recursos As New Resources.ResourceLoader()
        Interfaz.Pestañas.Visibilidad_Pestañas(gridConfig, recursos.GetString("Config"))

    End Sub

    Private Sub AbrirImagen1Click(sender As Object, e As RoutedEventArgs)

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim grid As Grid = pagina.FindName("gridConfigImagen1")
        Dim icono As FontAwesome5.FontAwesome = pagina.FindName("iconoConfigImagen1")

        If grid.Visibility = Visibility.Collapsed Then
            grid.Visibility = Visibility.Visible
            icono.Icon = FontAwesome5.EFontAwesomeIcon.Solid_AngleDoubleUp
        Else
            grid.Visibility = Visibility.Collapsed
            icono.Icon = FontAwesome5.EFontAwesomeIcon.Solid_AngleDoubleDown
        End If

    End Sub

    Private Sub AbrirImagen2Click(sender As Object, e As RoutedEventArgs)

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim grid As Grid = pagina.FindName("gridConfigImagen2")
        Dim icono As FontAwesome5.FontAwesome = pagina.FindName("iconoConfigImagen2")

        If grid.Visibility = Visibility.Collapsed Then
            grid.Visibility = Visibility.Visible
            icono.Icon = FontAwesome5.EFontAwesomeIcon.Solid_AngleDoubleUp
        Else
            grid.Visibility = Visibility.Collapsed
            icono.Icon = FontAwesome5.EFontAwesomeIcon.Solid_AngleDoubleDown
        End If

    End Sub

    Private Sub BuscarTwitchClick(sender As Object, e As RoutedEventArgs)

        Twitch.Generar(True)

    End Sub

    Public Sub Estado(estado As Boolean)

        Dim frame As Frame = Window.Current.Content
        Dim pagina As Page = frame.Content

        Dim botonAbrirConfig As Button = pagina.FindName("botonAbrirConfig")
        botonAbrirConfig.IsEnabled = estado

        Dim botonBuscarCarpetaTwitch As Button = pagina.FindName("botonBuscarCarpetaTwitch")
        botonBuscarCarpetaTwitch.IsEnabled = estado

    End Sub

End Module