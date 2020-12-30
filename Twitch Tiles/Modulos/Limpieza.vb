Imports System.Net

Module Limpieza

    Public Function Limpiar(texto As String)

        If Not texto = String.Empty Then
            texto = texto.Trim

            If texto.Contains("DLC") Then
                Dim int As Integer = texto.IndexOf("DLC")

                If int = texto.Length - 3 Then
                    texto = texto.Remove(texto.Length - 3, 3)
                End If
            End If

            texto = WebUtility.HtmlDecode(texto)

            Dim listaCaracteres As New List(Of String) From {"(Mac)", "(Mac & Linux)", "(Linux)", "(Steam)", "(Epic)", "(GOG)", "Early Access", "Pre Order", "Pre-Purchase",
                " ", "•", ">", "<", "¿", "?", "!", "¡", ":", "â", "Â", "¢",
                ".", "_", "–", "-", ";", ",", "™", "®", "'", "’", "´", "`", "(", ")", "/", "\", "|", "&", "#", "=", "+", ChrW(34),
                "@", "^", "[", "]", "ª", "«"}

            For Each item In listaCaracteres
                texto = texto.Replace(item, Nothing)
            Next

            texto = texto.ToLower
            texto = texto.Trim
        End If

        Return texto
    End Function

End Module
