Module Module1
    ''' <summary>
    ''' Loads the given directory to the datagrid view
    ''' </summary>
    ''' <param name="directory"></param>
    ''' <returns></returns>
    Public Function Fileinfo_To_DataTable(ByVal directory As String) As DataTable
        Try
            'Create a new data table
            Dim dt As DataTable = New DataTable

            'Add the following columns:
            '                          Name
            '                          Subject
            '                          From
            ''                         Size
            dt.Columns.AddRange({New DataColumn("Dir"), New DataColumn("Name"), New DataColumn("Subject"), New DataColumn("From"), New DataColumn("Size (KB)")})

            'Loop through each file in the directory
            For Each file As IO.FileInfo In New IO.DirectoryInfo(directory).GetFiles
                'Create a new row
                If file.Extension = ".eml" Then

                    Dim dr As DataRow = dt.NewRow

                    Dim message = MimeKit.MimeMessage.Load(file.FullName)

                    'Set the data
                    'Full directory, Filename, Subject, From, File size
                    dr(0) = file.FullName
                    dr(1) = file.Name
                    dr(2) = message.Subject
                    dr(3) = message.From
                    dr(4) = file.Length / 1000


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
