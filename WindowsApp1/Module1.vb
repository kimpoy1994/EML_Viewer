Module Module1
    Public Function Fileinfo_To_DataTable(ByVal directory As String) As DataTable
        Try
            'Create a new data table
            Dim dt As DataTable = New DataTable

            'Add the following columns:
            '                          Name
            '                          Length
            '                          Last Write Time
            ''                         Creation Time
            dt.Columns.AddRange({New DataColumn("Dir"), New DataColumn("Name"), New DataColumn("Size (KB)"), New DataColumn("Subject"), New DataColumn("From")})

            'Loop through each file in the directory
            For Each file As IO.FileInfo In New IO.DirectoryInfo(directory).GetFiles
                'Create a new row
                If file.Extension = ".eml" Then

                    Dim dr As DataRow = dt.NewRow

                    Dim message = MimeKit.MimeMessage.Load(file.FullName)

                    'Set the data
                    dr(0) = file.FullName
                    dr(1) = file.Name
                    dr(2) = file.Length / 1000
                    dr(3) = message.Subject
                    dr(4) = message.From

                    'Add the row to the data table
                    dt.Rows.Add(dr)
                End If

            Next

            'Return the data table
            Return dt
        Catch ex As Exception
            Console.WriteLine(ex.ToString)

            'Return nothing if something fails
            Return Nothing
        End Try
    End Function


End Module
