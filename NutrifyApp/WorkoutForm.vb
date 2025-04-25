Imports System.Data
Imports Npgsql
Imports System.Windows.Forms

Public Class WorkoutForm

    Private ReadOnly connStr As String =
        "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;" &
        "Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;" &
        "SSL Mode=Require;Trust Server Certificate=true"

    Private cboWorkouts As ComboBox
    Private nudMinutes As NumericUpDown
    Private btnLogWorkout As Button
    Private lstUserWorkouts As ListView
    Private lblStatus As Label
    Private workoutData As DataTable

    Private Const MaxRetries As Integer = 3

    Private dataLoaded As Boolean = False
    Private Shared sharedInstance As WorkoutForm = Nothing

    Public Shared Function GetInstance() As WorkoutForm
        If sharedInstance Is Nothing Then
            sharedInstance = New WorkoutForm()
        End If
        Return sharedInstance
    End Function

    Private Sub WorkoutForm_Load(sender As Object, e As EventArgs) _
        Handles MyBase.Load

        Me.Text = "Workouts"
        Me.Size = New Size(800, 600)
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(30, 30, 30)

        BuildUI()

        If Not dataLoaded Then
            lblStatus.Text = "Loading workouts..."
            BackgroundWorker1.RunWorkerAsync()
        Else
            PopulateWorkoutsCombo()
            LoadUserWorkoutLog()
        End If
    End Sub

    Private WithEvents BackgroundWorker1 As New System.ComponentModel.BackgroundWorker

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) _
        Handles BackgroundWorker1.DoWork

        LoadWorkoutDataWithRetry()
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) _
        Handles BackgroundWorker1.RunWorkerCompleted

        PopulateWorkoutsCombo()
        LoadUserWorkoutLogWithRetry()

        dataLoaded = True
    End Sub

    Private Sub LoadWorkoutDataWithRetry()
        Dim retries As Integer = 0
        Dim success As Boolean = False

        While Not success AndAlso retries < MaxRetries
            Try
                LoadWorkoutData()
                success = True
            Catch ex As Exception
                retries += 1
                If retries >= MaxRetries Then
                Else
                    System.Threading.Thread.Sleep(1000)
                End If
            End Try
        End While
    End Sub

    Private Sub LoadWorkoutData()
        Try
            Using conn As New NpgsqlConnection(connStr)
                conn.Open()
                Dim cmd As New NpgsqlCommand(
                    "SELECT workout_id, name, calories_burned " &
                    "FROM workouts ORDER BY name", conn)

                workoutData = New DataTable()
                Using adapter As New NpgsqlDataAdapter(cmd)
                    adapter.Fill(workoutData)
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub BuildUI()

        Dim lblTitle As New Label With {
            .Text = "Log a Workout",
            .Font = New Font("Segoe UI", 22, FontStyle.Bold),
            .ForeColor = Color.LimeGreen,
            .Location = New Point(20, 20),
            .AutoSize = True
        }
        Me.Controls.Add(lblTitle)

        ' ---------- combo: workouts ----------
        cboWorkouts = New ComboBox With {
            .Font = New Font("Segoe UI", 12),
            .Location = New Point(30, 90),
            .Width = 300,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
        Me.Controls.Add(cboWorkouts)

        ' ---------- numeric: minutes ----------
        nudMinutes = New NumericUpDown With {
            .Font = New Font("Segoe UI", 12),
            .Minimum = 1,
            .Maximum = 360,
            .Value = 30,
            .Location = New Point(350, 90),
            .Width = 120
        }
        Me.Controls.Add(nudMinutes)

        Dim lblMin As New Label With {
            .Text = "minutes",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(480, 93),
            .AutoSize = True
        }
        Me.Controls.Add(lblMin)

        ' ---------- log button ----------
        btnLogWorkout = New Button With {
            .Text = "Log Workout",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .BackColor = Color.LimeGreen,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(600, 85),
            .Size = New Size(140, 35)
        }
        AddHandler btnLogWorkout.Click, AddressOf btnLogWorkout_Click
        Me.Controls.Add(btnLogWorkout)

        ' ---------- status label ----------
        lblStatus = New Label With {
            .Font = New Font("Segoe UI", 11),
            .ForeColor = Color.White,
            .Location = New Point(30, 130),
            .AutoSize = True,
            .Text = ""
        }
        Me.Controls.Add(lblStatus)

        ' ---------- list‑view of user workouts ----------
        lstUserWorkouts = New ListView With {
            .View = View.Details,
            .Location = New Point(30, 170),
            .Size = New Size(710, 350),
            .FullRowSelect = True,
            .GridLines = True,
            .Font = New Font("Segoe UI", 11),
            .VirtualMode = False  ' Changed from True to False
        }
        lstUserWorkouts.Columns.Add("Date", 120)
        lstUserWorkouts.Columns.Add("Workout", 300)
        lstUserWorkouts.Columns.Add("Duration (min)", 140)
        lstUserWorkouts.Columns.Add("Calories", 140)
        Me.Controls.Add(lstUserWorkouts)

        ' ---------- back button ----------
        Dim btnBack As New Button With {
            .Text = "← Back",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = Color.DarkRed,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(680, 20),
            .Size = New Size(80, 30)
        }
        AddHandler btnBack.Click, AddressOf btnBack_Click
        Me.Controls.Add(btnBack)
    End Sub

    ' === Populate workout combo box from cached data =========================
    Private Sub PopulateWorkoutsCombo()
        If workoutData IsNot Nothing AndAlso workoutData.Rows.Count > 0 Then
            cboWorkouts.DataSource = workoutData
            cboWorkouts.DisplayMember = "name"
            cboWorkouts.ValueMember = "workout_id"
            lblStatus.Text = ""
        Else
            lblStatus.Text = "No workouts found!"
            lblStatus.ForeColor = Color.Red
        End If
    End Sub

    ' === Load user workout log with retry ====================================
    Private Sub LoadUserWorkoutLogWithRetry()
        Dim retries As Integer = 0
        Dim success As Boolean = False

        While Not success AndAlso retries < MaxRetries
            Try
                LoadUserWorkoutLog()
                success = True
            Catch ex As Exception
                retries += 1

                If retries >= MaxRetries Then
                    lblStatus.Text = "Failed to load workout log after multiple attempts"
                    lblStatus.ForeColor = Color.Red

                    ' Add a refresh button if all retries fail
                    Dim btnRefresh As New Button With {
                        .Text = "Retry Loading",
                        .Font = New Font("Segoe UI", 10),
                        .BackColor = Color.Orange,
                        .ForeColor = Color.Black,
                        .FlatStyle = FlatStyle.Flat,
                        .Location = New Point(350, 130),
                        .Size = New Size(120, 30),
                        .Tag = "RefreshButton"
                    }
                    AddHandler btnRefresh.Click, AddressOf btnRefresh_Click
                    Me.Controls.Add(btnRefresh)
                Else
                    ' Wait briefly before retrying
                    System.Threading.Thread.Sleep(1000)
                End If
            End Try
        End While
    End Sub

    ' === Refresh button click handler =======================================
    Private Sub btnRefresh_Click(sender As Object, e As EventArgs)
        ' Remove the refresh button if it exists
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Button AndAlso ctrl.Tag = "RefreshButton" Then
                Me.Controls.Remove(ctrl)
                Exit For
            End If
        Next

        lblStatus.Text = "Retrying to load workout history..."
        lblStatus.ForeColor = Color.White

        ' Try loading again
        LoadUserWorkoutLogWithRetry()
    End Sub

    ' === Load the current user's workout history into list‑view ===============
    Private Sub LoadUserWorkoutLog()
        lstUserWorkouts.Items.Clear()
        lblStatus.Text = "Loading workout history..."

        Try
            Using conn As New NpgsqlConnection(connStr)
                conn.Open()

                ' Use parameters and limit results for better performance
                Dim cmd As New NpgsqlCommand(
                    "SELECT uw.date_completed, w.name, uw.duration_minutes, " &
                    "       ROUND(w.calories_burned * uw.duration_minutes / 60.0) AS cals " &
                    "FROM   user_workouts uw " &
                    "JOIN   workouts w ON uw.workout_id = w.workout_id " &
                    "WHERE  uw.user_id = @uid " &
                    "ORDER BY uw.date_completed DESC " &
                    "LIMIT 100", conn)
                cmd.Parameters.AddWithValue("uid", CurrUserID)

                ' Use a data adapter for efficiency
                Dim adapter As New NpgsqlDataAdapter(cmd)
                Dim table As New DataTable()
                adapter.Fill(table)

                ' Populate the ListView from the DataTable
                For Each row As DataRow In table.Rows
                    Dim item As New ListViewItem(Convert.ToDateTime(row("date_completed")).ToString("yyyy-MM-dd")) ' Fixed hyphen
                    item.SubItems.Add(row("name").ToString())
                    item.SubItems.Add(row("duration_minutes").ToString())
                    item.SubItems.Add(row("cals").ToString())
                    lstUserWorkouts.Items.Add(item)
                Next

                lblStatus.Text = ""
            End Using
        Catch ex As Exception
            lblStatus.Text = "Failed to load workout log: " & ex.Message
            lblStatus.ForeColor = Color.Red
            Throw ' Re-throw so retry logic can catch it
        End Try
    End Sub

    ' === Click: Log Workout ===================================================
    Private Sub btnLogWorkout_Click(sender As Object, e As EventArgs)
        If cboWorkouts.SelectedIndex = -1 Then
            MessageBox.Show("Please choose a workout.")
            Return
        End If

        Dim workoutId As Integer = CInt(cboWorkouts.SelectedValue)
        Dim minutes As Integer = CInt(nudMinutes.Value)
        Dim userId As Integer = CurrUserID

        lblStatus.Text = "Logging workout..."
        lblStatus.ForeColor = Color.White

        ' Disable the button while processing
        btnLogWorkout.Enabled = False

        Try
            Using conn As New NpgsqlConnection(connStr)
                conn.Open()

                ' Use a transaction for atomicity
                Using transaction As NpgsqlTransaction = conn.BeginTransaction()
                    Try
                        ' 1. Insert into User_Workouts
                        Dim insertUW As New NpgsqlCommand(
                            "INSERT INTO user_workouts " &
                            "(user_id, workout_id, date_completed, duration_minutes) " &
                            "VALUES (@uid, @wid, CURRENT_DATE, @mins)", conn, transaction)
                        insertUW.Parameters.AddWithValue("uid", userId)
                        insertUW.Parameters.AddWithValue("wid", workoutId)
                        insertUW.Parameters.AddWithValue("mins", minutes)
                        insertUW.ExecuteNonQuery()

                        ' 2. Award 50 pts if first workout today
                        Dim check As New NpgsqlCommand(
                            "SELECT COUNT(*) FROM user_workouts " &
                            "WHERE user_id=@uid AND date_completed=CURRENT_DATE", conn, transaction)
                        check.Parameters.AddWithValue("uid", userId)
                        Dim countToday As Integer = CInt(check.ExecuteScalar())

                        If countToday = 1 Then
                            Dim award As New NpgsqlCommand(
                                "INSERT INTO points (user_id, points, date_recorded) " &
                                "VALUES (@uid, 50, CURRENT_DATE)", conn, transaction)
                            award.Parameters.AddWithValue("uid", userId)
                            award.ExecuteNonQuery()

                            transaction.Commit()
                            WorkoutLogtrue = True
                            lblStatus.ForeColor = Color.LimeGreen
                            lblStatus.Text = "Workout logged!"

                        Else
                            transaction.Commit()
                            lblStatus.ForeColor = Color.LimeGreen
                            lblStatus.Text = "Workout logged!"
                        End If
                    Catch ex As Exception
                        ' Rollback on error
                        transaction.Rollback()
                        Throw
                    End Try
                End Using
            End Using

            ' Refresh list‑view with retry capability
            LoadUserWorkoutLogWithRetry()

        Catch ex As Exception
            lblStatus.ForeColor = Color.Red
            lblStatus.Text = "Error logging workout: " & ex.Message
        Finally
            ' Re-enable the button
            btnLogWorkout.Enabled = True
        End Try
    End Sub

    ' === When Back button is clicked, hide instead of close ===
    Private Sub btnBack_Click(sender As Object, e As EventArgs)
        Dim dashboard As New UserDashboard()
        dashboard.Show()
        Me.Hide() ' Hide instead of Close()
    End Sub

    ' === Handle form closing to clean up singleton pattern ===
    Private Sub WorkoutForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
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