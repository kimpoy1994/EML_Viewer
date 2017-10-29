Imports System.Text.RegularExpressions
Public Class Form1
    Dim dgv_datasource
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Changing the colors of the label so that it is not visible on run
        txtSubject.BackColor = Me.BackColor
        txtFrom.BackColor = Me.BackColor
        'Get all the drive for the tree view
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
        'Get the directory of the double clicked node to fill up thee datagrid
        BackgroundWorker1.RunWorkerAsync(e.Node.FullPath)

    End Sub

    Private Sub DataGridView1_CellMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles DataGridView1.CellMouseClick
        If e.RowIndex >= 0 AndAlso e.ColumnIndex >= 0 Then
            Try
                ' Load the email file using the selected row
                Dim message = MimeKit.MimeMessage.Load(DataGridView1.Rows(e.RowIndex).Cells(0).Value)
                LoadEmail(message)

                'If directory is not found
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
                'Load the email file using the selected row
                Dim message = MimeKit.MimeMessage.Load(DataGridView1.Rows(DataGridView1.CurrentRow.Index - 1).Cells(0).Value)
                LoadEmail(message)

                'If the directory is not found
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
                'Load the email file using the selected row
                Dim message = MimeKit.MimeMessage.Load(DataGridView1.Rows(DataGridView1.CurrentRow.Index + 1).Cells(0).Value)
                LoadEmail(message)

                'If the directory is not found
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
        'Event handling to display hyperlink
        AddHandler WebBrowser1.Document.MouseOver, AddressOf Me.DisplayHyperlinks

        'So that the user will not be able to click URL from the email displayed
        WebBrowser1.AllowNavigation = False
    End Sub

    Private Sub DisplayHyperlinks(sender As Object, e As HtmlElementEventArgs)
        ' Shows the URL in the label below of the webbrowser control
        If e.ToElement.GetAttribute("href").Length = 0 Then
            ' Use this if the URL is not being displayed
            ' Example, the <a href=#><span>CLICK HERE</span></a>
            urlLabel.Text = e.ToElement.Parent.GetAttribute("href")
        Else
            'Example <a href=#>CLICK HERE</a>
            urlLabel.Text = e.ToElement.GetAttribute("href")
        End If

    End Sub

    ''' <summary>
    ''' Subroutine use in loading an email to the Web browser control
    ''' </summary>
    ''' <param name="email"></param>
    Private Sub LoadEmail(email)
        'Function for displaying the email in the web browser control
        WebBrowser1.AllowNavigation = True
        Dim htmlPart = email.HtmlBody
        txtSubject.Text = email.Subject
        txtFrom.Text = email.From.ToString

        If htmlPart = Nothing Then
            htmlPart = "<pre> " + email.TextBody + "</pre>"
        End If

        WebBrowser1.DocumentText = htmlPart

    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Try
            'Create a new data table
            Dim dt As DataTable = New DataTable
            'added somethins
            'Add the following columns:
            '                          Name
            '                          Subject
            '                          From
            ''                         Size
            dt.Columns.AddRange({New DataColumn("Dir"), New DataColumn("Name"), New DataColumn("Subject"), New DataColumn("From"), New DataColumn("Size (KB)")})

            'Loop through each file in the directory
            Dim counter = New IO.DirectoryInfo(e.Argument).GetFiles.Count
            Dim i As Integer = 0
            For Each file As IO.FileInfo In New IO.DirectoryInfo(e.Argument).GetFiles
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
                i += 1
                Dim state = New Integer() {i, counter}
                BackgroundWorker1.ReportProgress((i / counter) * 100, state)

            Next

            'Return the data table
            dgv_datasource = dt
        Catch ex As Exception
            Console.WriteLine(ex.ToString)

            'Return nothing if something fails
        End Try
    End Sub

    Private Sub BackgroundWorker1_Workcompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        UpdateDataGridView(dgv_datasource)
    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        ProgressBar1.Value = e.ProgressPercentage
        lblStatus.Text = e.UserState(0).ToString + " out of " + e.UserState(1).ToString
    End Sub

    Private Sub UpdateDataGridView(datasource)
        DataGridView1.DataSource = datasource
        Try
            'Remove the first column, the full directory
            DataGridView1.Columns(0).Visible = False
        Catch ex As Exception
            'TODO
        End Try
    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim itemPath As Array

    End Sub
End Class
