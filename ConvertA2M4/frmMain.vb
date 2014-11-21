Imports System.IO
''' <summary>
''' Dönüştürme işlemleri için ffMpeg uygulamasından yararlanıldı. İnternetteki en iyi uygulama bu. Hem açık kaynak hem doküman bol.
''' Ama harici bir uygulama olduğu için tek satırda "shell" komutu ile çalıştırıyoruz. progressbar için takip edilemez bir durum
''' Önceden AVI'den WMV'ye dönüştürüyorlarmış. ama sonradan küçük olsun diye mp4 uygun seçilmiş. Bu proje sıfırdan başlamış ve bitmiş bir projedir.
''' Hem tekli hem çoklu dönüştürme yapabiliyor. İstenilen adrese kaydeder.
''' ''' </summary>
''' <remarks>
''' 
''' created by Hüseyin Sekmenoğlu @ 2014
''' 
''' </remarks>
Public Class frmMain
    Dim proc As New Process 'durdurma işlemi yapabilmek için global bir değişken yapıyoruz
    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        If RadioButton1.Checked = True Then 'eğer tek bir dosya açacaksa dosya seç dialogu gelecek.
            dlgOpen.ShowDialog()
            If dlgOpen.FileName.Trim <> "" And dlgOpen.FileName.Trim <> txtFrom.Text Then txtFrom.Text = dlgOpen.FileName.Trim : txtTo.Text = txtFrom.Text.Replace(".avi", ".mp4") 'eğer dosya adı ok ise
        Else 'yoksa klasör seçtirip oradaki tüm avileri dönüştürecek.
            dlgFolder.ShowDialog() 'dönüştürme adresi ilk baştaki dosya ile aynı adreste  yapılır ama sonradan değiştirilebilir.
            If dlgFolder.SelectedPath <> "" And dlgFolder.SelectedPath <> txtFrom.Text Then txtFrom.Text = dlgFolder.SelectedPath & "\*.avi" : txtTo.Text = txtFrom.Text.Replace(".avi", ".mp4")
        End If
    End Sub
    Private Sub btnFolder_Click(sender As Object, e As EventArgs) Handles btnFolder.Click
        'önce dönüştürülecek dosyanın mp4 uzantılı adı bulunur.
        Dim ad() As String = txtFrom.Text.Split("\") : Dim ad2 As String = ad(UBound(ad)).Replace(".avi", ".mp4")
        'kayıt yeri klasör olarak seçilir. aynı isimle mp4 uzantılı olarak kaydedilecek.
        dlgFolder.ShowDialog()
        If dlgFolder.SelectedPath <> "" And dlgFolder.SelectedPath <> txtTo.Text Then txtTo.Text = dlgFolder.SelectedPath
        If Strings.Right(txtTo.Text, 1) = "\" Then txtTo.Text &= ad2 Else txtTo.Text &= "\" & ad2
    End Sub
    Private Sub btnConvert_Click(sender As Object, e As EventArgs) Handles btnConvert.Click
        If txtTo.Text = "" Then Exit Sub
        Me.Enabled = False
        If RadioButton1.Checked = True Then 'sadece tek bir dosya dönüştürülecek ise...
            If File.Exists(txtTo.Text) Then File.Delete(txtTo.Text) 'eğer dosya varsa sil
            Try 'bazen hata çıkarabiliyor. şu anda bir dakika içinde işlem tamamlanmazsa programı sonlandırıyor.
                Shell("ffmpeg -i """ & txtFrom.Text & """ -c:v libx264 -preset slow -crf 20 -c:a libvo_aacenc -b:a 128k """ & txtTo.Text & """", AppWinStyle.Hide, True, 60000)
            Catch ex As Exception
                For Each prog As Process In Process.GetProcesses 'eğer hata olursa ffmpeg işlemini durudur emri versin.
                    If prog.ProcessName = "ffmpeg" Then prog.Kill()
                Next
            End Try
        Else 'burada çoklu dönüştürme işlemi yapılacak
            Dim directory As New IO.DirectoryInfo(txtFrom.Text.Replace("\*.avi", "")) : Dim Files As IO.FileInfo() = directory.GetFiles() : Dim FileNames As IO.FileInfo : Dim eski(1) As String : Dim i As Integer = 0
            For Each FileNames In Files 'tüm aviler bir diziye atılır
                If FileNames.Extension = ".avi" Then eski(i) = FileNames.FullName : i += 1 : ReDim Preserve eski(i)
            Next
            For j = 0 To i - 1
                Dim ad() As String = eski(j).Split("\") : Dim ad2 As String = txtTo.Text.Replace("*.mp4", ad(UBound(ad))).Replace(".avi", ".mp4")
                If File.Exists(ad2) Then File.Delete(ad2) 'eğer dosya varsa sil
                Try 'bazen hata çıkarabiliyor. şu anda bir dakika içinde işlem tamamlanmazsa programı sonlandırıyor.
                    Shell("ffmpeg -i """ & eski(j) & """ -c:v libx264 -preset slow -crf 20 -c:a libvo_aacenc -b:a 128k """ & ad2 & """", AppWinStyle.Hide, True, 60000)
                Catch ex As Exception
                    For Each prog As Process In Process.GetProcesses 'eğer hata olursa ffmpeg işlemini durudur emri versin.
                        If prog.ProcessName = "ffmpeg" Then prog.Kill()
                    Next
                End Try
            Next
        End If
        Me.Enabled = True
        If MsgBox("Dönüştürme işlemi tamamlandı. Dosyayı açmak ister misiniz?", MsgBoxStyle.ApplicationModal + MsgBoxStyle.Information + MsgBoxStyle.YesNo, Application.ProductName) = MsgBoxResult.Yes Then
            If RadioButton1.Checked = True Then 'dönüştürme işlemi sonucu dosyayı aç
                Try
                    Process.Start(txtTo.Text)
                Catch ex As Exception
                    MsgBox("Video yok", MsgBoxStyle.Critical)
                End Try
            Else 'yada klasörü aç
                Dim ad() As String = txtTo.Text.Split("\") : Dim ad2 As String = txtTo.Text.Replace("\" & ad(UBound(ad)), "") : Process.Start("explorer.exe", txtTo.Text.Replace("\*.mp4", ""))
            End If
        End If
    End Sub
    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        RadioButton1.Checked = Not RadioButton2.Checked : txtFrom.Text = "" : txtTo.Text = "" 'ya biri aktif olsun ya diğeri
        If RadioButton2.Checked = True Then btnOpen.Text = "Klasör Seç" 'her halükarda textboxları silsin
    End Sub
    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        RadioButton2.Checked = Not RadioButton1.Checked : txtFrom.Text = "" : txtTo.Text = ""
        If RadioButton1.Checked = True Then btnOpen.Text = "Dosya Seç"
    End Sub
End Class
