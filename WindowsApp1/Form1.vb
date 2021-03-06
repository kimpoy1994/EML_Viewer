﻿Imports System.Text.RegularExpressions
Imports System
Imports System.IO
Public Class Form1

    Dim dgv_datasource
    Dim currentItem As String
    Dim currentPath As String
    Dim currentRow As Integer
    Dim NodesThatMatch As New List(Of TreeNode)
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Changing the colors of the label so that it is not visible on run
        txtSubject.BackColor = Me.BackColor
        txtFrom.BackColor = Me.BackColor
        lblStatus.Text = ""
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

    Private Sub TreeViewEmail_BeforeCollapse(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewCancelEventArgs) Handles treeViewEmail.BeforeCollapse
        ' clear the node that is being collapsed
        e.Node.Nodes.Clear()
        ' add a dummy TreeNode to the node being collapsed so it is expandable
        e.Node.Nodes.Add("*DUMMY*")
    End Sub

    Private Sub TreeViewEmail_BeforeExpand(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewCancelEventArgs) Handles treeViewEmail.BeforeExpand
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

    Private Sub TreeViewEmail_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles treeViewEmail.NodeMouseDoubleClick
        'Get the directory of the double clicked node to fill up thee datagrid
        If BackgroundWorker1.IsBusy Then
            BackgroundWorker1.CancelAsync()
            While BackgroundWorker1.IsBusy
                Application.DoEvents()
            End While
        End If
        BackgroundWorker1.RunWorkerAsync(e.Node.FullPath)
        currentPath = e.Node.FullPath

    End Sub

    Private Sub DataGridView1_CellMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles DataGridView1.CellMouseClick
        If e.RowIndex >= 0 AndAlso e.ColumnIndex >= 0 Then
            Try
                ' Load the email file using the selected row
                Dim message = MimeKit.MimeMessage.Load(DataGridView1.Rows(e.RowIndex).Cells(0).Value)
                Dim text = message.TextBody
                Dim html = message.HtmlBody
                Dim headers = message.Headers
                currentItem = DataGridView1.Rows(e.RowIndex).Cells(0).Value
                TextView.Text = text
                HTMLView.Text = html

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
        Do While True
            If BackgroundWorker1.CancellationPending = True Then
                e.Cancel = True
                Exit Do
            Else
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
                        If BackgroundWorker1.CancellationPending = True Then
                            Exit For
                        End If
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
                    Exit Do
                Catch ex As Exception
                    Console.WriteLine(ex.ToString)

                    'Return nothing if something fails
                End Try
            End If
        Loop
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

    Private Function SearchTreeView(ByVal TV As TreeView, ByVal TextToFind As String) As TreeNode
        'Empty previous
        NodesThatMatch.Clear()

        ' Keep calling Recursive search
        For Each TN As TreeNode In TV.Nodes
            If TN.FullPath.ToString = TextToFind Then
                NodesThatMatch.Add(TN)
            End If
            RecursiveSearch(TN, TextToFind)
        Next

        If NodesThatMatch.Count > 0 Then
            Return NodesThatMatch(0)
        Else
            Return Nothing
        End If
    End Function

    Private Sub RecursiveSearch(ByVal treeNode As TreeNode, ByVal TextToFind As String)
        ' Keep calling the test recursively
        For Each TN As TreeNode In treeNode.Nodes
            If TN.FullPath.ToString = TextToFind Then
                NodesThatMatch.Add(TN)
            End If

            RecursiveSearch(TN, TextToFind)
        Next
    End Sub

    Private Sub MoveMails(destpath As String, filename As String)
        For Each item In DataGridView1.SelectedRows

            Dim file = New FileInfo(item.Cells(0).Value.ToString)
            file.MoveTo(Path.Combine(destpath, file.Name))
        Next

        For Each item In DataGridView1.SelectedRows
            DataGridView1.Rows.RemoveAt(item.Index)
        Next

        Dim matchedNode = SearchTreeView(treeViewEmail, currentPath)

        matchedNode.Nodes.Clear()
        ' get the directory representing this node
        Dim mNodeDirectory As IO.DirectoryInfo
        mNodeDirectory = New IO.DirectoryInfo(matchedNode.Tag.ToString)
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
                matchedNode.Nodes.Add(mDirectoryNode)

                'additional comment
            Next
        Catch ex As Exception
            MessageBox.Show("Directory currently inaccessible!", "Directory Inaccessible")
        End Try
        matchedNode.Expand()

        Dim message = MimeKit.MimeMessage.Load(DataGridView1.Rows(currentRow).Cells(0).Value)
        LoadEmail(message)

    End Sub

    Private Sub BtnNormal_Click(sender As Object, e As EventArgs) Handles btnNormal.Click, BtnNormal2.Click
        Dim destPath As String = currentPath + "\Normal\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)

    End Sub

    Private Sub BtnSpam_Click(sender As Object, e As EventArgs) Handles BtnSpam.Click, BtnSpam2.Click
        Dim destPath As String = currentPath + "\Spam\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)

    End Sub

    Private Sub BtnMML_Click(sender As Object, e As EventArgs) Handles BtnMML.Click, BtnMML2.Click
        Dim destPath As String = currentPath + "\MML\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)

    End Sub

    Private Sub BtnInvalid_Click(sender As Object, e As EventArgs) Handles BtnInvalid.Click, BtnInvalid2.Click
        Dim destPath As String = currentPath + "\Invalid\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)

    End Sub

    Private Sub BtnAttach_Click(sender As Object, e As EventArgs) Handles BtnAttach.Click, BtnAttach2.Click
        Dim destPath As String = currentPath + "\Spam\AttachHash\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)

    End Sub

    Private Sub BtnTag_Click(sender As Object, e As EventArgs) Handles BtnTag.Click, BtnTag2.Click
        Dim destPath As String = currentPath + "\Spam\TagHash\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)

    End Sub

    Private Sub BtnText_Click(sender As Object, e As EventArgs) Handles BtnText.Click, BtnText2.Click
        Dim destPath As String = currentPath + "\Spam\TextHash\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)

    End Sub

    Private Sub BtnURL_Click(sender As Object, e As EventArgs) Handles BtnURL.Click, BtnURL2.Click
        Dim destPath As String = currentPath + "\Spam\URLHash\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)

    End Sub

    Private Sub btnPrev_Click(sender As Object, e As EventArgs) Handles btnPrev.Click
        Try
            'Load the email file using the selected row
            DataGridView1.Rows(currentRow).Selected = False
            DataGridView1.Rows(currentRow - 1).Selected = True
            If currentRow > 0 Then
                currentRow -= 1
            End If

            Dim message = MimeKit.MimeMessage.Load(DataGridView1.Rows(currentRow).Cells(0).Value)
            currentItem = DataGridView1.Rows(currentRow).Cells(0).Value
            Debug.WriteLine(currentRow)
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
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        Try
            'Load the email file using the selected row
            DataGridView1.Rows(currentRow).Selected = False
            DataGridView1.Rows(currentRow + 1).Selected = True
            If currentRow < DataGridView1.RowCount Then
                currentRow += 1
            End If
            Dim message = MimeKit.MimeMessage.Load(DataGridView1.Rows(currentRow).Cells(0).Value)
            currentItem = DataGridView1.Rows(currentRow).Cells(0).Value
            Debug.WriteLine(currentRow)
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
    End Sub

    Private Sub DataGridView1_SelectionChanged(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.RowEnter
        currentRow = e.RowIndex
        currentItem = DataGridView1.Rows(currentRow).Cells(0).Value
        Debug.WriteLine(e.RowIndex)
    End Sub

    Private Sub BtnHeuristics_Click(sender As Object, e As EventArgs) Handles BtnHeuristics.Click, BtnHeuristics2.Click
        Dim destPath As String = currentPath + "\Spam\Heuristics\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If
        MoveMails(destPath, filename)
    End Sub

    Private Sub BtnSignature_Click(sender As Object, e As EventArgs) Handles BtnSignature.Click, BtnSignature2.Click
        Dim destPath As String = currentPath + "\Spam\Signature\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)
    End Sub

    Private Sub BtnBlacklist_Click(sender As Object, e As EventArgs) Handles BtnBlacklist.Click, BtnBlacklist2.Click
        Dim destPath As String = currentPath + "\Spam\Blacklist\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)
    End Sub

    Private Sub BtnOthers_Click(sender As Object, e As EventArgs) Handles BtnOthers.Click
        Dim destPath As String = currentPath + "\Spam\Others\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)
    End Sub

    Private Sub BtnNoSoln_Click(sender As Object, e As EventArgs) Handles BtnNoSoln.Click
        Dim destPath As String = currentPath + "\Spam\No Solution\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim destPath As String = currentPath + "\Skip\"
        Dim filename As String = Path.GetFileName(currentItem)

        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        MoveMails(destPath, filename)

    End Sub

End Class
