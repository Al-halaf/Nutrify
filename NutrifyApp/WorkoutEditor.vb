Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data
Imports Npgsql
Imports System.Threading.Tasks
Imports System.ComponentModel
Imports System.Net.NetworkInformation

Public Class WorkoutEditor
    ' Colors
    Private PrimaryColor As Color = Color.LimeGreen
    Private ButtonColor As Color = Color.LimeGreen
    Private BackButtonColor As Color = Color.DarkRed
    Private ErrorColor As Color = Color.Red

    ' UI Controls
    Private WithEvents lblTitle As Label
    Private WithEvents lblName As Label
    Private WithEvents txtName As TextBox
    Private WithEvents lblCategory As Label
    Private WithEvents cboCategory As ComboBox
    Private WithEvents lblCalories As Label
    Private WithEvents txtCalories As TextBox
    Private WithEvents cmdSave As Button
    Private WithEvents cmdCancel As Button
    Private WithEvents lblStatus As Label

    ' Data
    Private WorkoutId As Integer
    Private IsNewWorkout As Boolean
    Private CurrentWorkout As DataRow = Nothing
    Private ValidationErrors As New List(Of String)
    Private IsLoading As Boolean = True
    Private ConnectionString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=2;Maximum Pool Size=10;Timeout=15;Command Timeout=15;"

    Public Sub New(workoutId As Integer)
        ' This call is required by the designer.
        InitializeComponentWE()

        ' Set the workout ID and determine if this is a new workout
        Me.WorkoutId = workoutId
        Me.IsNewWorkout = (workoutId = 0)

        ' Set the form title based on whether we're adding or editing
        Me.Text = If(IsNewWorkout, "Add New Workout", "Edit Workout")

        ' Load workout data or prepare for a new workout
        If IsNewWorkout Then
            Me.lblTitle.Text = "Add New Workout"
            IsLoading = False
        Else
            Me.lblTitle.Text = "Edit Workout"
            LoadWorkoutData()
        End If
    End Sub

    Private Sub InitializeComponentWE()
        Me.Size = New Size(550, 400)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(30, 30, 30)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        ' === Title Label ===
        lblTitle = New Label With {
            .Text = "Workout Details",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = PrimaryColor,
            .AutoSize = True,
            .Location = New Point(30, 20)
        }
        Me.Controls.Add(lblTitle)

        ' === Create Label-TextBox Pairs ===
        Dim yPosition As Integer = 80
        Dim spacing As Integer = 55

        ' === Name ===
        lblName = CreateLabel("Workout Name:", 30, yPosition)
        txtName = CreateTextBox(180, yPosition, 250)
        Me.Controls.Add(lblName)
        Me.Controls.Add(txtName)
        yPosition += spacing

        ' === Category ===
        lblCategory = CreateLabel("Category:", 30, yPosition)
        cboCategory = CreateComboBox(180, yPosition, 250)
        cboCategory.Items.AddRange(New String() {"Strength", "Cardio", "Flexibility", "HIIT", "Balance", "Core", "Yoga", "CrossFit", "Other"})
        Me.Controls.Add(lblCategory)
        Me.Controls.Add(cboCategory)
        yPosition += spacing

        ' === Calories ===
        lblCalories = CreateLabel("Calories Burned:", 30, yPosition)
        txtCalories = CreateTextBox(180, yPosition, 100)
        Me.Controls.Add(lblCalories)
        Me.Controls.Add(txtCalories)
        yPosition += spacing

        ' === Status Label ===
        lblStatus = New Label With {
            .Font = New Font("Segoe UI", 11),
            .ForeColor = Color.White,
            .Location = New Point(30, 250),
            .Size = New Size(480, 40),
            .Text = "Enter workout details and click Save."
        }
        Me.Controls.Add(lblStatus)

        ' === Save Button ===
        cmdSave = New Button With {
            .Text = "Save",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(300, 310),
            .Size = New Size(100, 36)
        }
        Me.Controls.Add(cmdSave)

        ' === Cancel Button ===
        cmdCancel = New Button With {
            .Text = "Cancel",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = BackButtonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(410, 310),
            .Size = New Size(100, 36)
        }
        Me.Controls.Add(cmdCancel)

        ' Add event handlers
        AddHandler cmdSave.Click, AddressOf cmdSave_Click
        AddHandler cmdCancel.Click, AddressOf cmdCancel_Click
    End Sub

    Private Function CreateLabel(text As String, x As Integer, y As Integer) As Label
        Return New Label With {
            .Text = text,
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(x, y),
            .AutoSize = True
        }
    End Function

    Private Function CreateTextBox(x As Integer, y As Integer, width As Integer, Optional height As Integer = 25, Optional multiline As Boolean = False) As TextBox
        Return New TextBox With {
            .Font = New Font("Segoe UI", 11),
            .Location = New Point(x, y),
            .Width = width,
            .Height = height,
            .BackColor = Color.FromArgb(45, 45, 45),
            .ForeColor = Color.White,
            .Multiline = multiline
        }
    End Function

    Private Function CreateComboBox(x As Integer, y As Integer, width As Integer) As ComboBox
        Return New ComboBox With {
            .Font = New Font("Segoe UI", 11),
            .Location = New Point(x, y),
            .Width = width,
            .BackColor = Color.FromArgb(45, 45, 45),
            .ForeColor = Color.White,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
    End Function

    Private Async Sub LoadWorkoutData()
        IsLoading = True
        Me.Cursor = Cursors.WaitCursor
        lblStatus.Text = "Loading workout data..."

        Try
            ' Check network connectivity first
            If Not CheckNetworkConnectivity() Then
                lblStatus.Text = "Network connection issue. Please check your internet connection."
                lblStatus.ForeColor = ErrorColor
                IsLoading = False
                Me.Cursor = Cursors.Default
                Return
            End If

            ' Try to load workout data with timeout
            Dim loadTask = Task.Run(Function() GetWorkoutFromDatabase(WorkoutId))

            ' Add timeout to the task
            If Await Task.WhenAny(loadTask, Task.Delay(8000)) Is loadTask Then
                ' Task completed within timeout
                Dim workoutData As DataRow = loadTask.Result

                If workoutData IsNot Nothing Then
                    CurrentWorkout = workoutData

                    ' Populate the form fields
                    txtName.Text = Convert.ToString(workoutData("name"))

                    If Not IsDBNull(workoutData("category")) Then
                        Dim category As String = Convert.ToString(workoutData("category"))
                        If cboCategory.Items.Contains(category) Then
                            cboCategory.SelectedItem = category
                        Else
                            cboCategory.Items.Add(category)
                            cboCategory.SelectedItem = category
                        End If
                    End If

                    If Not IsDBNull(workoutData("calories_burned")) Then
                        txtCalories.Text = Convert.ToString(workoutData("calories_burned"))
                    End If

                    lblStatus.Text = $"Editing {txtName.Text}"
                Else
                    lblStatus.Text = "Could not find the selected workout in the database."
                    lblStatus.ForeColor = ErrorColor
                End If
            Else
                ' Task timed out
                lblStatus.Text = "Database connection timed out. Please try again later."
                lblStatus.ForeColor = ErrorColor
            End If

        Catch ex As Exception
            HandleDatabaseException(ex, "loading workout data")
        Finally
            Me.Cursor = Cursors.Default
            IsLoading = False
        End Try
    End Sub

    Private Function CheckNetworkConnectivity() As Boolean
        Try
            ' Try to ping Google's DNS to check internet connectivity
            Using ping As New Ping()
                Dim reply As PingReply = ping.Send("8.8.8.8", 3000)
                Return (reply.Status = IPStatus.Success)
            End Using
        Catch ex As Exception
            ' If any exception occurs during ping, assume network is down
            Return False
        End Try
    End Function

    Private Function GetWorkoutFromDatabase(workoutId As Integer) As DataRow
        Try
            Using conn As New NpgsqlConnection(ConnectionString)
                ' Set short timeouts to avoid long waits
                conn.Open()

                Dim query As String = "SELECT * FROM Workouts WHERE workout_id = @workout_id;"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 5  ' Set command timeout to 5 seconds
                    cmd.Parameters.AddWithValue("@workout_id", workoutId)

                    Using reader = cmd.ExecuteReader()
                        Dim workoutTable As New DataTable()
                        workoutTable.Load(reader)
                        If workoutTable.Rows.Count > 0 Then
                            Return workoutTable.Rows(0)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As NpgsqlException
            ' Log the specific database error but don't throw it
            System.Diagnostics.Debug.WriteLine("Database error: " & ex.Message)
            If ex.InnerException IsNot Nothing Then
                System.Diagnostics.Debug.WriteLine("Inner exception: " & ex.InnerException.Message)
            End If
            ' Let the caller handle the null return
        Catch ex As Exception
            ' Log general errors
            System.Diagnostics.Debug.WriteLine("General error: " & ex.Message)
        End Try

        Return Nothing
    End Function

    Private Sub cmdCancel_Click(sender As Object, e As EventArgs)
        Me.DialogResult = DialogResult.Cancel

        Me.Close()

    End Sub

    Private Async Sub cmdSave_Click(sender As Object, e As EventArgs)
        If ValidateInput() = False Then
            ' Show the validation errors
            lblStatus.Text = String.Join("; ", ValidationErrors)
            lblStatus.ForeColor = ErrorColor
            Return
        End If

        Me.Cursor = Cursors.WaitCursor
        lblStatus.Text = "Saving workout data..."
        lblStatus.ForeColor = Color.White

        Try
            ' Check network connectivity first
            If Not CheckNetworkConnectivity() Then
                lblStatus.Text = "Network connection issue. Please check your internet connection."
                lblStatus.ForeColor = ErrorColor
                Me.Cursor = Cursors.Default
                Return
            End If

            ' Get the data from the form
            Dim name As String = txtName.Text.Trim()
            Dim category As String = If(cboCategory.SelectedItem IsNot Nothing, cboCategory.SelectedItem.ToString(), "")
            Dim calories As Integer? = ParseNullableInteger(txtCalories.Text)

            ' Try to save with timeout
            Dim saveTask As Task(Of Boolean)
            If IsNewWorkout Then
                saveTask = Task.Run(Function() InsertWorkoutIntoDatabase(name, category, calories))
            Else
                saveTask = Task.Run(Function() UpdateWorkoutInDatabase(WorkoutId, name, category, calories))
            End If

            ' Add timeout to the save task
            If Await Task.WhenAny(saveTask, Task.Delay(8000)) Is saveTask Then
                ' Task completed within timeout
                Dim success As Boolean = saveTask.Result

                If success Then
                    Me.DialogResult = DialogResult.OK
                    Me.Close()
                Else
                    lblStatus.Text = "Error saving data to the database. Please try again."
                    lblStatus.ForeColor = ErrorColor
                End If
            Else
                ' Task timed out
                lblStatus.Text = "Database operation timed out. Please try again later."
                lblStatus.ForeColor = ErrorColor
            End If

        Catch ex As Exception
            HandleDatabaseException(ex, "saving workout data")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub HandleDatabaseException(ex As Exception, operation As String)
        Dim errorMessage As String = $"Error {operation}: "

        If TypeOf ex Is NpgsqlException Then
            Dim npgEx As NpgsqlException = DirectCast(ex, NpgsqlException)
            errorMessage &= "Database connection problem. "

            If npgEx.InnerException IsNot Nothing Then
                If TypeOf npgEx.InnerException Is System.Net.Sockets.SocketException Then
                    errorMessage &= "Network connection issue. Please check your internet connection."
                ElseIf TypeOf npgEx.InnerException Is System.IO.IOException Then
                    errorMessage &= "Connection timed out. Please try again later."
                Else
                    errorMessage &= "Please try again later."
                End If
            Else
                errorMessage &= "Please try again later."
            End If
        Else
            errorMessage &= ex.Message
        End If

        System.Diagnostics.Debug.WriteLine(errorMessage)
        System.Diagnostics.Debug.WriteLine(ex.ToString())

        lblStatus.Text = errorMessage
        lblStatus.ForeColor = ErrorColor
    End Sub

    Private Function ValidateInput() As Boolean
        ValidationErrors.Clear()

        ' Required field validation
        If String.IsNullOrWhiteSpace(txtName.Text) Then
            ValidationErrors.Add("Workout name is required")
        End If

        If cboCategory.SelectedIndex = -1 Then
            ValidationErrors.Add("Category is required")
        End If

        ' Numeric field validation
        CheckNumericField(txtCalories.Text, "Calories burned")

        Return ValidationErrors.Count = 0
    End Function

    Private Sub CheckNumericField(value As String, fieldName As String)
        If Not String.IsNullOrWhiteSpace(value) Then
            Dim numValue As Integer
            If Not Integer.TryParse(value, numValue) Then
                ValidationErrors.Add($"{fieldName} must be a whole number")
            ElseIf numValue < 0 Then
                ValidationErrors.Add($"{fieldName} cannot be negative")
            End If
        End If
    End Sub

    Private Function ParseNullableInteger(text As String) As Integer?
        If String.IsNullOrWhiteSpace(text) Then
            Return Nothing
        End If

        Dim result As Integer
        If Integer.TryParse(text, result) Then
            Return result
        End If

        Return Nothing
    End Function

    Private Function InsertWorkoutIntoDatabase(
        name As String, category As String, calories As Integer?) As Boolean

        Try
            Using conn As New NpgsqlConnection(ConnectionString)
                conn.Open()

                Dim query As String = "
                    INSERT INTO Workouts (
                        name, category, calories_burned
                    ) VALUES (
                        @name, @category, @calories
                    );"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 5  ' Short timeout
                    cmd.Parameters.AddWithValue("@name", name)

                    If String.IsNullOrEmpty(category) Then
                        cmd.Parameters.AddWithValue("@category", DBNull.Value)
                    Else
                        cmd.Parameters.AddWithValue("@category", category)
                    End If

                    AddNullableParameter(cmd, "@calories", calories)

                    cmd.ExecuteNonQuery()
                    Return True
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error inserting workout: " & ex.Message)
            If TypeOf ex Is NpgsqlException AndAlso ex.InnerException IsNot Nothing Then
                System.Diagnostics.Debug.WriteLine("Inner exception: " & ex.InnerException.Message)
            End If
            Return False
        End Try
    End Function

    Private Function UpdateWorkoutInDatabase(
        workoutId As Integer, name As String, category As String, calories As Integer?) As Boolean

        Try
            Using conn As New NpgsqlConnection(ConnectionString)
                conn.Open()

                Dim query As String = "
                    UPDATE Workouts SET
                        name = @name,
                        category = @category,
                        calories_burned = @calories
                    WHERE workout_id = @workout_id;"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 5  ' Short timeout
                    cmd.Parameters.AddWithValue("@workout_id", workoutId)
                    cmd.Parameters.AddWithValue("@name", name)

                    If String.IsNullOrEmpty(category) Then
                        cmd.Parameters.AddWithValue("@category", DBNull.Value)
                    Else
                        cmd.Parameters.AddWithValue("@category", category)
                    End If

                    AddNullableParameter(cmd, "@calories", calories)

                    cmd.ExecuteNonQuery()
                    Return True
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error updating workout: " & ex.Message)
            If TypeOf ex Is NpgsqlException AndAlso ex.InnerException IsNot Nothing Then
                System.Diagnostics.Debug.WriteLine("Inner exception: " & ex.InnerException.Message)
            End If
            Return False
        End Try
    End Function

    Private Sub AddNullableParameter(cmd As NpgsqlCommand, paramName As String, value As Object)
        If value Is Nothing Then
            cmd.Parameters.AddWithValue(paramName, DBNull.Value)
        Else
            cmd.Parameters.AddWithValue(paramName, value)
        End If
    End Sub
End Class