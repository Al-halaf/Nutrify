' EditWorkoutForm.vb
Imports Npgsql

Public Class EditWorkoutForm
    Inherits System.Windows.Forms.Form

    ' Form controls
    Friend WithEvents lblName As Label
    Friend WithEvents txtName As TextBox
    Friend WithEvents lblCategory As Label
    Friend WithEvents cboCategory As ComboBox
    Friend WithEvents lblCalories As Label
    Friend WithEvents numCalories As NumericUpDown
    Friend WithEvents btnSave As Button
    Friend WithEvents btnCancel As Button

    ' Connection string for database - replace with your secure connection method
    Dim connectionString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;"

    Private ReadOnly workoutId As Integer

    Public Sub New(id As Integer)
        InitializeComponent()
        workoutId = id
        LoadWorkoutData()
    End Sub


    ' Initialize the components
    Private Sub InitializeComponent()
        Me.lblName = New System.Windows.Forms.Label()
        Me.txtName = New System.Windows.Forms.TextBox()
        Me.lblCategory = New System.Windows.Forms.Label()
        Me.cboCategory = New System.Windows.Forms.ComboBox()
        Me.lblCalories = New System.Windows.Forms.Label()
        Me.numCalories = New System.Windows.Forms.NumericUpDown()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        CType(Me.numCalories, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        ' lblName
        '
        Me.lblName.AutoSize = True
        Me.lblName.Location = New System.Drawing.Point(30, 30)
        Me.lblName.Name = "lblName"
        Me.lblName.Size = New System.Drawing.Size(45, 17)
        Me.lblName.TabIndex = 0
        Me.lblName.Text = "Name:"
        '
        ' txtName
        '
        Me.txtName.Location = New System.Drawing.Point(150, 30)
        Me.txtName.Name = "txtName"
        Me.txtName.Size = New System.Drawing.Size(200, 22)
        Me.txtName.TabIndex = 1
        '
        ' lblCategory
        '
        Me.lblCategory.AutoSize = True
        Me.lblCategory.Location = New System.Drawing.Point(30, 70)
        Me.lblCategory.Name = "lblCategory"
        Me.lblCategory.Size = New System.Drawing.Size(69, 17)
        Me.lblCategory.TabIndex = 2
        Me.lblCategory.Text = "Category:"
        '
        ' cboCategory
        '
        Me.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboCategory.FormattingEnabled = True
        Me.cboCategory.Items.AddRange(New Object() {"Cardio", "Strength", "Flexibility", "Balance", "HIIT"})
        Me.cboCategory.Location = New System.Drawing.Point(150, 70)
        Me.cboCategory.Name = "cboCategory"
        Me.cboCategory.Size = New System.Drawing.Size(200, 24)
        Me.cboCategory.TabIndex = 3
        '
        ' lblCalories
        '
        Me.lblCalories.AutoSize = True
        Me.lblCalories.Location = New System.Drawing.Point(30, 110)
        Me.lblCalories.Name = "lblCalories"
        Me.lblCalories.Size = New System.Drawing.Size(114, 17)
        Me.lblCalories.TabIndex = 4
        Me.lblCalories.Text = "Calories Burned:"
        '
        ' numCalories
        '
        Me.numCalories.Location = New System.Drawing.Point(150, 110)
        Me.numCalories.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        Me.numCalories.Name = "numCalories"
        Me.numCalories.Size = New System.Drawing.Size(120, 22)
        Me.numCalories.TabIndex = 5
        '
        ' btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(100, 160)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(100, 35)
        Me.btnSave.TabIndex = 6
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        ' btnCancel
        '
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Location = New System.Drawing.Point(220, 160)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(100, 35)
        Me.btnCancel.TabIndex = 7
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        ' EditWorkoutForm
        '
        Me.AcceptButton = Me.btnSave
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnCancel
        Me.ClientSize = New System.Drawing.Size(382, 220)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.numCalories)
        Me.Controls.Add(Me.lblCalories)
        Me.Controls.Add(Me.cboCategory)
        Me.Controls.Add(Me.lblCategory)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.lblName)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "EditWorkoutForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Edit Workout"
        CType(Me.numCalories, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    ' Form load event
    Private Sub EditWorkoutForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Load workout data into form
        LoadWorkoutData()
    End Sub

    ' Load workout data from database
    Private Sub LoadWorkoutData()
        Try
            Using conn As New NpgsqlConnection(connectionString)
                conn.Open()

                ' Get workout data
                Dim cmd As New NpgsqlCommand("SELECT name, category, calories_burned FROM Workouts WHERE workout_id = @id", conn)
                cmd.Parameters.AddWithValue("@id", workoutId)

                Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        ' Populate form controls with data
                        txtName.Text = reader("name").ToString()
                        cboCategory.SelectedItem = reader("category").ToString()
                        numCalories.Value = Convert.ToDecimal(reader("calories_burned"))
                    Else
                        MessageBox.Show("Workout not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Me.Close()
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading workout data: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
        End Try
    End Sub

    ' Save button click event
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        ' Validate input
        If String.IsNullOrWhiteSpace(txtName.Text) Then
            MessageBox.Show("Please enter a workout name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtName.Focus()
            Return
        End If

        If cboCategory.SelectedIndex = -1 Then
            MessageBox.Show("Please select a category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cboCategory.Focus()
            Return
        End If

        ' Update workout in database
        If UpdateWorkout() Then
            ' Set DialogResult to OK to indicate success to the parent form
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End If
    End Sub

    ' Cancel button click event
    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    ' Function to update the workout in the database
    Private Function UpdateWorkout() As Boolean
        Try
            Using conn As New NpgsqlConnection(connectionString)
                conn.Open()

                ' Update workout
                Dim cmd As New NpgsqlCommand("UPDATE Workouts SET name = @name, category = @category, calories_burned = @calories WHERE workout_id = @id", conn)
                cmd.Parameters.AddWithValue("@name", txtName.Text)
                cmd.Parameters.AddWithValue("@category", cboCategory.SelectedItem.ToString())
                cmd.Parameters.AddWithValue("@calories", CInt(numCalories.Value))
                cmd.Parameters.AddWithValue("@id", workoutId)

                Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                If rowsAffected > 0 Then
                    MessageBox.Show("Workout updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return True
                Else
                    MessageBox.Show("No workout was updated. The workout may have been deleted.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return False
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error updating workout: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function
End Class