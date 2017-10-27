Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each drive As IO.DriveInfo In IO.DriveInfo.GetDrives()
            Dim mRootNode As New TreeNode
            mRootNode.Tag = drive
            mRootNode.Text = drive.ToString()
            mRootNode.Nodes.Add("*DUMMY*")
            treeViewEmail.Nodes.Add(mRootNode)
        Next
        'mRootNode.Text = RootPath
        'mRootNode.Tag = RootPath
        'mRootNode.Nodes.Add("*DUMMY*")
        'treeViewEmail.Nodes.Add(mRootNode)
    End Sub

    Private Sub treeViewEmail_BeforeCollapse(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewCancelEventArgs) Handles treeViewEmail.BeforeCollapse
        ' clear the node that is being collapsed
        e.Node.Nodes.Clear()
        ' add a dummy TreeNode to the node being collapsed so it is expandable
        e.Node.Nodes.Add("*DUMMY*")
    End Sub

    Private Sub treeViewEmail_BeforeExpand(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewCancelEventArgs) Handles treeViewEmail.BeforeExpand
        ' clear the expanding node so we can re-populate it, or else we end up with duplicate nodes
        e.Node.Nodes.Clear()
        ' get the directory representing this node
        Dim mNodeDirectory As IO.DirectoryInfo
        mNodeDirectory = New IO.DirectoryInfo(e.Node.Tag.ToString)
        Try
            ' add each subdirectory from the file system to the expanding node as a child node
            For Each mDirectory As IO.DirectoryInfo In mNodeDirectory.GetDirectories
                ' declare a child TreeNode for the next subdirectory
                Dim mDirectoryNode As New TreeNode
                ' store the full path to this directory in the child TreeNode's Tag property
                mDirectoryNode.Tag = mDirectory.FullName
                ' set the child TreeNodes's display text
                mDirectoryNode.Text = mDirectory.Name
                ' add a dummy TreeNode to this child TreeNode to make it expandable
                mDirectoryNode.Nodes.Add("*DUMMY*")
                ' add this child TreeNode to the expanding TreeNode
                e.Node.Nodes.Add(mDirectoryNode)

                'additional comment
            Next
        Catch ex As Exception
            MessageBox.Show("Directory currently inaccessible!", "Directory Inaccessible")
        End Try

    End Sub

    Private Sub treeViewEmail_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles treeViewEmail.NodeMouseDoubleClick
        DataGridView1.DataSource = Fileinfo_To_DataTable(e.Node.FullPath)
        DataGridView1.Columns(0).Visible = False
    End Sub

    Private Sub DataGridView1_CellMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles DataGridView1.CellMouseClick
        If e.RowIndex >= 0 AndAlso e.ColumnIndex >= 0 Then
            ' Dim selectedRow = DataGridView1.Rows(e.RowIndex)
            'Dim message = LoadEmlFromFile(DataGridView1.Rows(e.RowIndex).Cells(0).Value)
            Try
                Dim message = MimeKit.MimeMessage.Load(DataGridView1.Rows(e.RowIndex).Cells(0).Value)

                'Debug.WriteLine(message.HtmlBody)
                Dim htmlPart = message.HtmlBody

                If htmlPart = Nothing Then
                    htmlPart = "<pre> " + message.TextBody + "</pre>"
                End If

                WebBrowser1.DocumentText = htmlPart
            Catch ex As IO.DirectoryNotFoundException
                MessageBox.Show("File not found!")
                Me.Controls.Clear()
                InitializeComponent()
                Form1_Load(e, e)
            End Try

        End If
    End Sub

    Private Sub DataGridView1_KeyDown(sender As Object, e As KeyEventArgs) Handles DataGridView1.KeyDown
        If e.KeyCode = Keys.Up Then
            Try
                'Dim selectedRow = DataGridView1.Rows(DataGridView1.CurrentRow.Index + 1)
                'Dim message = LoadEmlFromFile(DataGridView1.Rows(e.RowIndex).Cells(0).Value)
                Dim message = MimeKit.MimeMessage.Load(DataGridView1.Rows(DataGridView1.CurrentRow.Index - 1).Cells(0).Value)
                Debug.WriteLine(DataGridView1.CurrentRow.Index + 1)

                'Debug.WriteLine(message.HtmlBody)
                Dim htmlPart = message.HtmlBody

                If htmlPart = Nothing Then
                    htmlPart = "<pre> " + message.TextBody + "</pre>"
                End If

                WebBrowser1.DocumentText = htmlPart
            Catch ex As IO.DirectoryNotFoundException
                MessageBox.Show("File not found!")
                Me.Controls.Clear()
                InitializeComponent()
                Form1_Load(e, e)
            Catch ex As Exception
                'TODO
            End Try
        ElseIf e.KeyCode = Keys.Down Then
            Try
                Dim message = MimeKit.MimeMessage.Load(DataGridView1.Rows(DataGridView1.CurrentRow.Index + 1).Cells(0).Value)
                Debug.WriteLine(DataGridView1.CurrentRow.Index - 1)
                'Debug.WriteLine(message.HtmlBody)
                Dim htmlPart = message.HtmlBody

                If htmlPart = Nothing Then
                    htmlPart = "<pre> " + message.TextBody + "</pre>"
                End If

                WebBrowser1.DocumentText = htmlPart
            Catch ex As IO.DirectoryNotFoundException
                MessageBox.Show("File not found!")
                Me.Controls.Clear()
                InitializeComponent()
                Form1_Load(e, e)
            Catch ex As Exception
                'TODO
            End Try
        End If
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        AddHandler WebBrowser1.Document.MouseOver, AddressOf Me.DisplayHyperlinks
    End Sub

    Private Sub DisplayHyperlinks(sender As Object, e As HtmlElementEventArgs)
        urlLabel.Text = e.ToElement.GetAttribute("href")
    End Sub

End Class
