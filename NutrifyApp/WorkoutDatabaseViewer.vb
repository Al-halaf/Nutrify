Imports Microsoft.VisualBasic.ApplicationServices
Imports Npgsql
Imports System.Windows.Forms
Imports System.Threading.Tasks
Imports System.Diagnostics
Imports System.Data
Imports System.Collections.Generic
Imports System.Threading

Public Class WorkoutDatabaseViewer
    ' Colors
    Private PrimaryColor As Color = Color.LimeGreen
    Private ButtonColor As Color = Color.LimeGreen
    Private BackButtonColor As Color = Color.DarkRed
    Private ErrorColor As Color = Color.Red

    ' UI Controls
    Private WithEvents lblTitle As Label
    Private WithEvents lblSearch As Label
    Private WithEvents txtSearch As TextBox
    Private WithEvents cboSearchType As ComboBox
    Private WithEvents cmdSearch As Button
    Private WithEvents cmdClearSearch As Button
    Private WithEvents lblStatus As Label
    Private WithEvents dataGridWorkouts As DataGridView
    Private WithEvents cmdBack As Button
    Private WithEvents cmdAddWorkout As Button
    Private WithEvents cmdEditWorkout As Button

    ' Data
    Private AllWorkoutsData As DataTable = Nothing
    Private FilteredData As DataView = Nothing
    Private DatabaseOperationInProgress As Boolean = False
    Private CancellationSource As New CancellationTokenSource()

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent1()

        ' Add any initialization after the InitializeComponent() call.
        InitializeUI()
        LoadAllWorkoutsFromDatabase()
    End Sub

    Private Sub InitializeComponent1()
        Me.Text = "Workout Database Viewer"
        Me.Size = New Size(950, 670)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(30, 30, 30)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
    End Sub

    Private Sub InitializeUI()
        Me.Controls.Clear()

        ' === Title Label ===
        lblTitle = New Label With {
            .Text = "Workout Database",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = PrimaryColor,
            .AutoSize = True,
            .Location = New Point(30, 20)
        }
        Me.Controls.Add(lblTitle)

        ' === Search Type Label ===
        lblSearch = New Label With {
            .Text = "Search By:",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(30, 80),
            .AutoSize = True
        }
        Me.Controls.Add(lblSearch)

        ' === Search Type ComboBox ===
        cboSearchType = New ComboBox With {
            .Font = New Font("Segoe UI", 11),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Location = New Point(130, 76),
            .Width = 120
        }
        cboSearchType.Items.AddRange(New String() {"Name", "Category"})
        cboSearchType.SelectedIndex = 0  ' Default to "Name"
        Me.Controls.Add(cboSearchType)

        ' === Search TextBox ===
        txtSearch = New TextBox With {
            .Font = New Font("Segoe UI", 11),
            .Location = New Point(260, 76),
            .Width = 300
        }
        Me.Controls.Add(txtSearch)

        ' === Search Button ===
        cmdSearch = New Button With {
            .Text = "Search",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(570, 75),
            .Size = New Size(100, 32)
        }
        Me.Controls.Add(cmdSearch)

        ' === Clear Search Button ===
        cmdClearSearch = New Button With {
            .Text = "Clear",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = BackButtonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(680, 75),
            .Size = New Size(100, 32),
            .Visible = False
        }
        Me.Controls.Add(cmdClearSearch)

        ' === Status Label ===
        lblStatus = New Label With {
            .Font = New Font("Segoe UI", 11),
            .ForeColor = Color.White,
            .Location = New Point(30, 130),
            .AutoSize = True,
            .Text = "Loading workout database..."
        }
        Me.Controls.Add(lblStatus)

        ' === DataGridView for Workouts ===
        dataGridWorkouts = New DataGridView With {
            .Location = New Point(30, 160),
            .Size = New Size(880, 400),
            .BackgroundColor = Color.FromArgb(45, 45, 45),
            .BorderStyle = BorderStyle.None,
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            .RowHeadersVisible = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .MultiSelect = False,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False
        }

        ' Configure grid appearance
        dataGridWorkouts.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45)
        dataGridWorkouts.DefaultCellStyle.ForeColor = Color.White
        dataGridWorkouts.DefaultCellStyle.SelectionBackColor = PrimaryColor
        dataGridWorkouts.DefaultCellStyle.SelectionForeColor = Color.White
        dataGridWorkouts.DefaultCellStyle.Font = New Font("Segoe UI", 10)

        dataGridWorkouts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(55, 55, 55)
        dataGridWorkouts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dataGridWorkouts.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        dataGridWorkouts.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        dataGridWorkouts.ColumnHeadersHeight = 40

        dataGridWorkouts.EnableHeadersVisualStyles = False
        Me.Controls.Add(dataGridWorkouts)

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
        Me.Controls.Add(cmdBack)

        ' === Add Workout Button ===
        cmdAddWorkout = New Button With {
            .Text = "Add New Workout",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(690, 580),
            .Size = New Size(120, 32)
        }
        Me.Controls.Add(cmdAddWorkout)

        ' === Edit Workout Button ===
        cmdEditWorkout = New Button With {
            .Text = "Edit Workout",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(820, 580),
            .Size = New Size(90, 32)
        }
        Me.Controls.Add(cmdEditWorkout)

        ' Add event handlers
        AddHandler cmdSearch.Click, AddressOf cmdSearch_Click
        AddHandler cmdClearSearch.Click, AddressOf cmdClearSearch_Click
        AddHandler cmdBack.Click, AddressOf cmdBack_Click
        AddHandler cmdAddWorkout.Click, AddressOf cmdAddWorkout_Click
        AddHandler cmdEditWorkout.Click, AddressOf cmdEditWorkout_Click
        AddHandler txtSearch.TextChanged, AddressOf txtSearch_TextChanged
    End Sub

    Private Sub WorkoutDatabaseViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Any additional load operations
    End Sub

    Private Sub WorkoutDatabaseViewer_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        CancellationSource.Cancel()
    End Sub

    Private Async Sub LoadAllWorkoutsFromDatabase()
        Me.Cursor = Cursors.WaitCursor
        lblStatus.Text = "Loading workout database..."
        lblStatus.Visible = True

        Try
            ' Create our data table with the correct schema
            AllWorkoutsData = New DataTable()
            AllWorkoutsData.Columns.Add("workout_id", GetType(Integer))
            AllWorkoutsData.Columns.Add("name", GetType(String))
            AllWorkoutsData.Columns.Add("category", GetType(String))
            AllWorkoutsData.Columns.Add("calories_burned", GetType(Integer))

            ' Make columns nullable where appropriate
            AllWorkoutsData.Columns("calories_burned").AllowDBNull = True

            ' Run the database query in a background thread
            Await Task.Run(Function() LoadWorkoutsFromDatabaseAsync())

            ' Now that we have all data, bind it to the grid
            DisplayWorkoutsData(AllWorkoutsData)

            lblStatus.Text = $"Found {AllWorkoutsData.Rows.Count} workouts in the database."

        Catch ex As Exception
            Debug.WriteLine("Error loading data: " & ex.Message)
            lblStatus.Text = "Error loading workout database. Please try again later."
            lblStatus.ForeColor = ErrorColor
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Async Function LoadWorkoutsFromDatabaseAsync() As Task
        ' Note: Remove the internet check as requested
        Dim connString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;"
        Dim maxRetries As Integer = 3
        Dim retryCount As Integer = 0

        While retryCount < maxRetries
            Try
                Using conn As New NpgsqlConnection(connString)
                    Await conn.OpenAsync()

                    Dim query As String = "SELECT * FROM Workouts ORDER BY name ASC;"

                    Using cmd As New NpgsqlCommand(query, conn)
                        Using reader = Await cmd.ExecuteReaderAsync()
                            While Await reader.ReadAsync()
                                Try
                                    Dim row As DataRow = AllWorkoutsData.NewRow()

                                    ' Get workout_id (should never be null)
                                    If Not reader.IsDBNull(reader.GetOrdinal("workout_id")) Then
                                        row("workout_id") = reader.GetInt32(reader.GetOrdinal("workout_id"))
                                    Else
                                        Continue While
                                    End If

                                    ' Get name (should never be null)
                                    If Not reader.IsDBNull(reader.GetOrdinal("name")) Then
                                        row("name") = reader.GetString(reader.GetOrdinal("name"))
                                    Else
                                        row("name") = "Unknown Workout"
                                    End If

                                    ' Get category (should never be null)
                                    If Not reader.IsDBNull(reader.GetOrdinal("category")) Then
                                        row("category") = reader.GetString(reader.GetOrdinal("category"))
                                    Else
                                        row("category") = "Uncategorized"
                                    End If

                                    ' Get calories burned (can be null)
                                    If Not reader.IsDBNull(reader.GetOrdinal("calories_burned")) Then
                                        row("calories_burned") = reader.GetInt32(reader.GetOrdinal("calories_burned"))
                                    End If

                                    AllWorkoutsData.Rows.Add(row)
                                Catch ex As Exception
                                    Debug.WriteLine("Error processing row: " & ex.Message)
                                End Try
                            End While
                        End Using
                    End Using
                End Using

                Return

            Catch ex As Exception
                Debug.WriteLine($"Database load attempt {retryCount + 1} failed: {ex.Message}")
                retryCount += 1

                Task.Run(Sub()
                             System.Threading.Thread.Sleep(500 * retryCount)
                         End Sub).Wait()
            End Try
        End While

        Debug.WriteLine("All database load attempts failed")
    End Function

    Private Sub DisplayWorkoutsData(dataTable As DataTable)
        Try
            dataGridWorkouts.DataSource = Nothing
            dataGridWorkouts.Columns.Clear()

            If dataTable Is Nothing OrElse dataTable.Rows.Count = 0 Then
                lblStatus.Text = "No workouts found in the database."
                Return
            End If

            ' Set the data source
            FilteredData = New DataView(dataTable)
            FilteredData.Sort = "name ASC"
            dataGridWorkouts.DataSource = FilteredData

            ' Configure the columns
            dataGridWorkouts.Columns("workout_id").Visible = False
            dataGridWorkouts.Columns("name").HeaderText = "Workout Name"
            dataGridWorkouts.Columns("name").Width = 300
            dataGridWorkouts.Columns("category").HeaderText = "Category"
            dataGridWorkouts.Columns("category").Width = 200
            dataGridWorkouts.Columns("calories_burned").HeaderText = "Calories (1 hour)"
            dataGridWorkouts.Columns("calories_burned").Width = 150

            ' Add a note about the fixed duration
            Dim note As New DataGridViewTextBoxColumn()
            note.HeaderText = "Duration"
            note.Width = 150
            note.ReadOnly = True
            note.DefaultCellStyle.ForeColor = Color.LightGray
            dataGridWorkouts.Columns.Add(note)

            ' Set the fixed duration value for all rows
            For Each row As DataGridViewRow In dataGridWorkouts.Rows
                If Not row.IsNewRow Then
                    row.Cells("Duration").Value = "1 hour (fixed)"
                End If
            Next

            ' Finish configuration
            dataGridWorkouts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None

        Catch ex As Exception
            Debug.WriteLine("Error displaying data: " & ex.Message)
            lblStatus.Text = "Error displaying workout data."
            lblStatus.ForeColor = ErrorColor
        End Try
    End Sub

    Private Sub ApplySearch()
        If AllWorkoutsData Is Nothing OrElse AllWorkoutsData.Rows.Count = 0 Then
            Return
        End If

        Dim searchText As String = txtSearch.Text.Trim()
        If String.IsNullOrEmpty(searchText) Then
            DisplayWorkoutsData(AllWorkoutsData)
            cmdClearSearch.Visible = False
            lblStatus.Text = $"Found {AllWorkoutsData.Rows.Count} workouts in the database."
            Return
        End If

        cmdClearSearch.Visible = True
        Me.Cursor = Cursors.WaitCursor

        Try
            Dim searchField As String = If(cboSearchType.SelectedIndex = 0, "name", "category")
            Dim filterExpression As String

            If searchField = "name" Then
                filterExpression = $"name LIKE '%{searchText}%'"
            Else
                filterExpression = $"category LIKE '%{searchText}%'"
            End If

            FilteredData = New DataView(AllWorkoutsData)
            FilteredData.RowFilter = filterExpression
            FilteredData.Sort = "name ASC"
            dataGridWorkouts.DataSource = FilteredData

            lblStatus.Text = $"Found {FilteredData.Count} workouts matching '{searchText}'."
            lblStatus.ForeColor = Color.White

        Catch ex As Exception
            Debug.WriteLine("Error applying search: " & ex.Message)
            lblStatus.Text = "Error applying search filter."
            lblStatus.ForeColor = ErrorColor
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub cmdSearch_Click(sender As Object, e As EventArgs)
        ApplySearch()
    End Sub

    Private Sub cmdClearSearch_Click(sender As Object, e As EventArgs)
        txtSearch.Clear()
        DisplayWorkoutsData(AllWorkoutsData)
        cmdClearSearch.Visible = False
        lblStatus.Text = $"Found {AllWorkoutsData.Rows.Count} workouts in the database."
        lblStatus.ForeColor = Color.White
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs)
        ' Auto search if text is empty or when 3+ characters are entered
        If txtSearch.Text.Length = 0 OrElse txtSearch.Text.Length >= 3 Then
            ApplySearch()
        End If
    End Sub

    Private Sub cmdBack_Click(sender As Object, e As EventArgs)
        ' Return to the main form
        Me.Close()
        AdminDashboard.Show()
    End Sub

    Private Sub cmdAddWorkout_Click(sender As Object, e As EventArgs)
        ShowWorkoutEditor(0) ' 0 indicates a new workout
    End Sub

    Private Sub cmdEditWorkout_Click(sender As Object, e As EventArgs)
        ' Check if a row is selected
        If dataGridWorkouts.SelectedRows.Count = 0 Then
            MessageBox.Show("Please select a workout to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Get the selected workout ID
        Dim selectedRow As DataGridViewRow = dataGridWorkouts.SelectedRows(0)
        Dim workoutId As Integer = Convert.ToInt32(selectedRow.Cells("workout_id").Value)

        ShowWorkoutEditor(workoutId)
    End Sub

    Private Sub ShowWorkoutEditor(workoutId As Integer)
        Dim workoutEditor As New WorkoutEditor(workoutId)
        If workoutEditor.ShowDialog() = DialogResult.OK Then
            ' Reload the workout database to reflect changes
            LoadAllWorkoutsFromDatabase()
        End If
    End Sub

    Private Sub ShowToast(message As String)
        Dim toast As New Form()
        toast.Text = ""
        toast.FormBorderStyle = FormBorderStyle.None
        toast.BackColor = Color.FromArgb(60, 60, 60)
        toast.ForeColor = Color.White
        toast.Opacity = 0.9
        toast.ShowInTaskbar = False
        toast.TopMost = True
        toast.StartPosition = FormStartPosition.Manual

        toast.Size = New Size(300, 50)
        toast.Location = New Point(
            Me.Location.X + Me.Width - toast.Width - 20,
            Me.Location.Y + Me.Height - toast.Height - 20)

        Dim label As New Label()
        label.Text = message
        label.AutoSize = False
        label.TextAlign = ContentAlignment.MiddleCenter
        label.Dock = DockStyle.Fill
        label.Font = New Font(label.Font.FontFamily, 10)
        toast.Controls.Add(label)

        Dim timer As New System.Windows.Forms.Timer()
        timer.Interval = 2000
        AddHandler timer.Tick, Sub(s, e)
                                   timer.Stop()
                                   toast.Close()
                                   timer.Dispose()
                               End Sub

        toast.Show()
        timer.Start()
    End Sub
End Class