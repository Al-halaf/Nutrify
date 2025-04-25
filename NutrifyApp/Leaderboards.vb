Imports Npgsql
Imports System.Data

Public Class Leaderboards

    Private lblNoResults As Label
    Private WithEvents txtUsernameSearch As TextBox
    Private WithEvents cboTimeRange As ComboBox
    Private WithEvents cmdSearch As Button
    Private WithEvents cmdBack As Button
    Private WithEvents listLeaderboard As ListView
    Private lblStatus As Label

    ' === Connection string (same one used elsewhere) ===
    Private ReadOnly connString As String =
        "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;" &
        "Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;" &
        "SSL Mode=Require;Trust Server Certificate=true"

    ' Data caching and retry functionality
    Private allPointsTable As New DataTable()
    Private dataLoaded As Boolean = False
    Private Shared sharedInstance As Leaderboards = Nothing
    Private Const MaxRetries As Integer = 3

    ' UI colors to match WorkoutForm
    Private ReadOnly BackgroundColor As Color = Color.FromArgb(30, 30, 30)
    Private ReadOnly PrimaryColor As Color = Color.LimeGreen
    Private ReadOnly ErrorColor As Color = Color.Red
    Private ReadOnly ButtonColor As Color = Color.LimeGreen
    Private ReadOnly BackButtonColor As Color = Color.DarkRed

    ' === Singleton pattern ===
    Public Shared Function GetInstance() As Leaderboards
        If sharedInstance Is Nothing Then
            sharedInstance = New Leaderboards()
        End If
        Return sharedInstance
    End Function

    ' === Form load event ===
    Private Sub Leaderboards_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "🏆 Leaderboards"
        Me.Size = New Size(900, 680)
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = BackgroundColor

        InitializeUI()

        ' Only load data if not already loaded
        If Not dataLoaded Then
            ' Load data asynchronously to prevent UI freeze
            BackgroundWorker1.RunWorkerAsync()
        Else
            ' Data is already loaded, just filter and display
            FilterAndDisplayLeaderboard()
        End If
    End Sub

    ' === Background worker for loading data ===
    Private WithEvents BackgroundWorker1 As New System.ComponentModel.BackgroundWorker

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) _
        Handles BackgroundWorker1.DoWork

        ' Load data with retry
        LoadAllPointsWithRetry()
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) _
        Handles BackgroundWorker1.RunWorkerCompleted

        ' Apply data to UI when background work completes
        FilterAndDisplayLeaderboard()

        ' Mark data as loaded
        dataLoaded = True
        lblStatus.Text = ""
    End Sub

    ' === Initialize UI ===
    Private Sub InitializeUI()
        Me.Controls.Clear()

        ' === Title Label ===
        Dim lblTitle As New Label With {
            .Text = "Leaderboard",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = PrimaryColor,
            .AutoSize = True,
            .Location = New Point(30, 20)
        }
        Me.Controls.Add(lblTitle)

        ' === Time Range Label ===
        Dim lblFilter As New Label With {
            .Text = "Time Range:",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(30, 80),
            .AutoSize = True
        }
        Me.Controls.Add(lblFilter)

        ' === ComboBox ===
        cboTimeRange = New ComboBox With {
            .Font = New Font("Segoe UI", 11),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Location = New Point(130, 76),
            .Width = 180
        }
        cboTimeRange.Items.AddRange(New String() {"Today", "This Week", "This Month", "This Year", "All Time"})
        cboTimeRange.SelectedIndex = 4  ' Default to "All Time"
        Me.Controls.Add(cboTimeRange)

        ' === Username Label ===
        Dim lblUsername As New Label With {
            .Text = "Username:",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(460, 80),
            .AutoSize = True
        }
        Me.Controls.Add(lblUsername)

        ' === Username TextBox ===
        txtUsernameSearch = New TextBox With {
            .Font = New Font("Segoe UI", 11),
            .Location = New Point(550, 76),
            .Width = 200
        }
        Me.Controls.Add(txtUsernameSearch)

        ' === Search Button ===
        cmdSearch = New Button With {
            .Text = "Search",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(330, 75),
            .Size = New Size(100, 32)
        }
        AddHandler cmdSearch.Click, AddressOf cmdSearch_Click
        Me.Controls.Add(cmdSearch)

        ' === Status Label ===
        lblStatus = New Label With {
            .Font = New Font("Segoe UI", 11),
            .ForeColor = Color.White,
            .Location = New Point(30, 130),
            .AutoSize = True,
            .Text = "Loading leaderboard data..."
        }
        Me.Controls.Add(lblStatus)

        ' === ListView ===
        listLeaderboard = New ListView With {
            .View = View.Details,
            .Font = New Font("Segoe UI", 11),
            .FullRowSelect = True,
            .GridLines = True,
            .Location = New Point(30, 160),
            .Size = New Size(820, 370)
        }
        listLeaderboard.Columns.Add("Rank", 100, HorizontalAlignment.Left)
        listLeaderboard.Columns.Add("Username", 480, HorizontalAlignment.Left)
        listLeaderboard.Columns.Add("Points", 200, HorizontalAlignment.Left)
        Me.Controls.Add(listLeaderboard)

        ' === "No Results" Label ===
        lblNoResults = New Label With {
            .Text = "No results found.",
            .Font = New Font("Segoe UI", 12, FontStyle.Italic),
            .ForeColor = ErrorColor,
            .Location = New Point(30, 540),
            .AutoSize = True,
            .Visible = False
        }
        Me.Controls.Add(lblNoResults)

        ' === Back Button ===
        cmdBack = New Button With {
            .Text = "← Back",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = BackButtonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(30, 580),
            .Size = New Size(100, 32)
        }
        AddHandler cmdBack.Click, AddressOf cmdBack_Click
        Me.Controls.Add(cmdBack)
    End Sub

    ' === Load all points with retry functionality ===
    Private Sub LoadAllPointsWithRetry()
        Dim retries As Integer = 0
        Dim success As Boolean = False

        While Not success AndAlso retries < MaxRetries
            Try
                LoadAllPoints()
                success = True
            Catch ex As Exception
                retries += 1
                If retries >= MaxRetries Then
                    ' Update UI from the UI thread
                    Me.Invoke(Sub()
                                  lblStatus.Text = "Failed to load leaderboard data after multiple attempts"
                                  lblStatus.ForeColor = ErrorColor

                                  ' Add a refresh button
                                  Dim btnRefresh As New Button With {
                                      .Text = "Retry Loading",
                                      .Font = New Font("Segoe UI", 10),
                                      .BackColor = Color.Orange,
                                      .ForeColor = Color.Black,
                                      .FlatStyle = FlatStyle.Flat,
                                      .Location = New Point(400, 130),
                                      .Size = New Size(120, 30),
                                      .Tag = "RefreshButton"
                                  }
                                  AddHandler btnRefresh.Click, AddressOf btnRefresh_Click
                                  Me.Controls.Add(btnRefresh)
                              End Sub)
                Else
                    ' Wait briefly before retrying
                    System.Threading.Thread.Sleep(1000)
                End If
            End Try
        End While
    End Sub

    ' === Refresh button click handler ===
    Private Sub btnRefresh_Click(sender As Object, e As EventArgs)
        ' Remove the refresh button if it exists
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Button AndAlso ctrl.Tag = "RefreshButton" Then
                Me.Controls.Remove(ctrl)
                Exit For
            End If
        Next

        lblStatus.Text = "Retrying to load leaderboard data..."
        lblStatus.ForeColor = Color.White

        ' Try loading again
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    ' === Load all points data from database ===
    Private Sub LoadAllPoints()
        Try
            If allPointsTable Is Nothing Then
                allPointsTable = New DataTable()
            Else
                allPointsTable.Clear()
            End If

            Using conn As New NpgsqlConnection(connString)
                conn.Open()

                Dim query As String = "
                    SELECT u.username, p.points, p.date_recorded
                    FROM Points p
                    JOIN Users u ON p.user_id = u.user_id
                    WHERE u.is_admin = FALSE
                "

                Using cmd As New NpgsqlCommand(query, conn)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(allPointsTable)
                    End Using
                End Using
            End Using

        Catch ex As Exception
            ' Let the retry logic handle it
            Throw
        End Try
    End Sub

    ' === Filter and display leaderboard with preserved ranks ===
    Private Sub FilterAndDisplayLeaderboard()
        listLeaderboard.BeginUpdate()
        listLeaderboard.Items.Clear()

        Try
            If allPointsTable Is Nothing OrElse allPointsTable.Rows.Count = 0 Then
                lblNoResults.Visible = True
                Return
            End If

            Dim filter As String = cboTimeRange.SelectedItem.ToString()
            Dim filteredRows As IEnumerable(Of DataRow) = allPointsTable.AsEnumerable()

            Dim cutoffDate As Date = Date.Now

            Select Case filter
                Case "Today"
                    cutoffDate = Date.Today
                    filteredRows = filteredRows.Where(Function(row) CDate(row("date_recorded")) = cutoffDate)
                Case "This Week"
                    cutoffDate = Date.Today.AddDays(-7)
                    filteredRows = filteredRows.Where(Function(row) CDate(row("date_recorded")) >= cutoffDate)
                Case "This Month"
                    cutoffDate = Date.Today.AddMonths(-1)
                    filteredRows = filteredRows.Where(Function(row) CDate(row("date_recorded")) >= cutoffDate)
                Case "This Year"
                    cutoffDate = Date.Today.AddYears(-1)
                    filteredRows = filteredRows.Where(Function(row) CDate(row("date_recorded")) >= cutoffDate)
                Case "All Time"
                    ' No filter
            End Select

            ' Group by username and sum points
            Dim globalRanking = From row In filteredRows
                                Group row By username = row.Field(Of String)("username") Into Group
                                Let total = Group.Sum(Function(r) r.Field(Of Integer)("points"))
                                Order By total Descending
                                Select username, total

            ' Compute ranks (handling ties) once for all users
            Dim rankings As New Dictionary(Of String, Integer)()  ' Username -> Rank
            Dim rankMap As New Dictionary(Of Integer, Integer)()  ' Points -> Rank
            Dim currentRank As Integer = 1

            For Each user In globalRanking
                If Not rankMap.ContainsKey(user.total) Then
                    rankMap(user.total) = currentRank
                End If

                rankings(user.username) = rankMap(user.total)
                currentRank += 1
            Next

            ' Username search filter (but preserve global rankings)
            Dim usernameQuery As String = txtUsernameSearch.Text.Trim().ToLower()
            Dim displayUsers = If(usernameQuery <> "",
                              globalRanking.Where(Function(u) u.username.ToLower().Contains(usernameQuery)),
                              globalRanking)

            ' Display with preserved ranks
            For Each user In displayUsers
                Dim item As New ListViewItem(rankings(user.username).ToString())
                item.SubItems.Add(user.username)
                item.SubItems.Add(user.total.ToString())
                listLeaderboard.Items.Add(item)
            Next

            lblNoResults.Visible = (listLeaderboard.Items.Count = 0)

        Catch ex As Exception
            lblStatus.Text = "Error displaying leaderboard: " & ex.Message
            lblStatus.ForeColor = ErrorColor
        Finally
            listLeaderboard.EndUpdate()
        End Try
    End Sub

    ' === Event handlers ===
    Private Sub cmdSearch_Click(sender As Object, e As EventArgs)
        FilterAndDisplayLeaderboard()
    End Sub

    Private Sub cboTimeRange_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboTimeRange.SelectedIndexChanged
        If dataLoaded Then
            FilterAndDisplayLeaderboard()
        End If
    End Sub

    Private Sub txtUsernameSearch_TextChanged(sender As Object, e As EventArgs) Handles txtUsernameSearch.TextChanged
        If dataLoaded Then
            FilterAndDisplayLeaderboard()
        End If
    End Sub

    Private Sub cmdBack_Click(sender As Object, e As EventArgs)
        Me.Hide()
        UserDashboard.Show()
    End Sub

    ' === Handle form closing to clean up singleton pattern ===
    Private Sub Leaderboards_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' If the form is being closed by the application ending,
        ' then null out the shared instance
        If e.CloseReason = CloseReason.ApplicationExitCall Or
           e.CloseReason = CloseReason.WindowsShutDown Or
           e.CloseReason = CloseReason.TaskManagerClosing Then
            sharedInstance = Nothing
        ElseIf e.CloseReason = CloseReason.UserClosing Then
            ' If user clicks the X button, hide instead of close
            e.Cancel = True
            Me.Hide()
        End If
    End Sub
End Class