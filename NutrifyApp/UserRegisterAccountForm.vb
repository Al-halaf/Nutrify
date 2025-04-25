Imports System.Text.RegularExpressions
Imports Npgsql
Imports BCrypt.Net

Public Class UserRegisterAccountForm
    Private existingUsernames As New HashSet(Of String)
    Private existingEmails As New HashSet(Of String)

    Private Sub UserRegisterAccountForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Register"
        Me.Size = New Size(600, 600) ' Adjusted form width to make it wider
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(30, 30, 30)

        InitializeUI()
        LoadUsernamesAndEmailsFromDatabase()
    End Sub

    Private Sub InitializeUI()
        ' Define consistent spacing values
        Dim leftMargin As Integer = 60
        Dim rightMargin As Integer = 200
        Dim controlWidth As Integer = 300
        Dim verticalSpacing As Integer = 70
        Dim labelOffset As Integer = 35
        Dim statusOffset As Integer = 30

        ' Define starting vertical position
        Dim currentY As Integer = 30

        ' Title
        Dim lblTitle As New Label With {
        .Text = "Create Account",
        .Font = New Font("Segoe UI", 24, FontStyle.Bold),
        .ForeColor = Color.LimeGreen,
        .Location = New Point(leftMargin + 65, currentY),
        .AutoSize = True
    }
        Me.Controls.Add(lblTitle)

        ' Move down for first field
        currentY += 80

        ' Username section
        Dim lblUsername As New Label With {
        .Text = "Username",
        .Font = New Font("Segoe UI", 12),
        .ForeColor = Color.White,
        .Location = New Point(leftMargin, currentY),
        .AutoSize = True
    }
        Me.Controls.Add(lblUsername)

        txtUsername = New TextBox With {
        .Location = New Point(rightMargin, currentY - 5),
        .Size = New Size(controlWidth, 30),
        .Font = New Font("Segoe UI", 12)
    }
        AddHandler txtUsername.TextChanged, AddressOf txtUsername_TextChanged
        Me.Controls.Add(txtUsername)

        lblUsernameStatus = New Label With {
        .Location = New Point(rightMargin, currentY + statusOffset),
        .AutoSize = True,
        .ForeColor = Color.White,
        .Font = New Font("Segoe UI", 9)
    }
        Me.Controls.Add(lblUsernameStatus)

        ' Email section
        currentY += verticalSpacing
        Dim lblEmail As New Label With {
        .Text = "Email",
        .Font = New Font("Segoe UI", 12),
        .ForeColor = Color.White,
        .Location = New Point(leftMargin, currentY),
        .AutoSize = True
    }
        Me.Controls.Add(lblEmail)

        txtEmail = New TextBox With {
        .Location = New Point(rightMargin, currentY - 5),
        .Size = New Size(controlWidth, 30),
        .Font = New Font("Segoe UI", 12)
    }
        AddHandler txtEmail.TextChanged, AddressOf txtEmail_TextChanged
        Me.Controls.Add(txtEmail)

        lblEmailStatus = New Label With {
        .Location = New Point(rightMargin, currentY + statusOffset),
        .AutoSize = True,
        .ForeColor = Color.White,
        .Font = New Font("Segoe UI", 9)
    }
        Me.Controls.Add(lblEmailStatus)

        ' Password section
        currentY += verticalSpacing
        Dim lblPassword As New Label With {
        .Text = "Password",
        .Font = New Font("Segoe UI", 12),
        .ForeColor = Color.White,
        .Location = New Point(leftMargin, currentY),
        .AutoSize = True
    }
        Me.Controls.Add(lblPassword)

        txtPassword = New TextBox With {
        .Location = New Point(rightMargin, currentY - 5),
        .Size = New Size(controlWidth, 30),
        .Font = New Font("Segoe UI", 12),
        .PasswordChar = "●"c
    }
        AddHandler txtPassword.TextChanged, AddressOf txtPassword_TextChanged
        Me.Controls.Add(txtPassword)

        lblPasswordStatus = New Label With {
        .Location = New Point(rightMargin, currentY + statusOffset),
        .AutoSize = True,
        .ForeColor = Color.White,
        .Font = New Font("Segoe UI", 9)
    }
        Me.Controls.Add(lblPasswordStatus)

        ' Date of Birth section
        currentY += verticalSpacing
        Dim lblDob As New Label With {
        .Text = "Date of Birth",
        .Font = New Font("Segoe UI", 12),
        .ForeColor = Color.White,
        .Location = New Point(leftMargin, currentY),
        .AutoSize = True
    }
        Me.Controls.Add(lblDob)

        dtpDob = New DateTimePicker With {
        .Location = New Point(rightMargin, currentY - 5),
        .Size = New Size(controlWidth, 30),
        .Font = New Font("Segoe UI", 12),
        .Format = DateTimePickerFormat.Short
    }
        Me.Controls.Add(dtpDob)

        ' Buttons - centered with proper spacing
        currentY += verticalSpacing + 20

        ' Register Button
        Dim btnRegister As New Button With {
        .Text = "Register",
        .Font = New Font("Segoe UI", 12, FontStyle.Bold),
        .BackColor = Color.LimeGreen,
        .ForeColor = Color.Black,
        .FlatStyle = FlatStyle.Flat,
        .Size = New Size(controlWidth, 40),
        .Location = New Point((Me.Width - controlWidth) \ 2, currentY)
    }
        AddHandler btnRegister.Click, AddressOf btnRegister_Click
        Me.Controls.Add(btnRegister)

        ' Back Button
        currentY += 55
        Dim btnBack As New Button With {
        .Text = "Back",
        .Font = New Font("Segoe UI", 12, FontStyle.Bold),
        .BackColor = Color.DimGray,
        .ForeColor = Color.White,
        .FlatStyle = FlatStyle.Flat,
        .Size = New Size(controlWidth, 40),
        .Location = New Point((Me.Width - controlWidth) \ 2, currentY)
    }
        AddHandler btnBack.Click, AddressOf btnBack_Click
        Me.Controls.Add(btnBack)
    End Sub


    ' Controls
    Private txtUsername As TextBox
    Private txtEmail As TextBox
    Private txtPassword As TextBox
    Private lblUsernameStatus As Label
    Private lblEmailStatus As Label
    Private lblPasswordStatus As Label
    Private dtpDob As DateTimePicker

    Private Const MaxRetryAttempts As Integer = 3
    Private Const RetryDelayMs As Integer = 1000
    Private Sub LoadUsernamesAndEmailsFromDatabase()
        Dim connStr As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true"
        Dim query As String = "SELECT username, email FROM users"
        Dim attemptCount As Integer = 0
        Dim success As Boolean = False

        While Not success AndAlso attemptCount < MaxRetryAttempts
            attemptCount += 1

            Using conn As New NpgsqlConnection(connStr)
                Try
                    conn.Open()
                    Dim cmd As New NpgsqlCommand(query, conn)

                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            existingUsernames.Add(reader.GetString(0).ToLower())
                            If reader.IsDBNull(1) = False Then
                                existingEmails.Add(reader.GetString(1).ToLower())
                            End If
                        End While
                    End Using

                    success = True ' Connection successful, exit retry loop
                Catch ex As Exception
                    ' Log the error internally (could write to log file)
                    Debug.WriteLine($"Database connection attempt {attemptCount} failed: {ex.Message}")

                    If attemptCount < MaxRetryAttempts Then
                        ' Wait before retrying
                        System.Threading.Thread.Sleep(RetryDelayMs)
                    End If
                End Try
            End Using
        End While

        If Not success Then
            ' If all retries failed, handle silently without showing error to user
            ' Could add a subtle indication in the UI that data might not be fully loaded
            lblUsernameStatus.Text = "Please enter your desired username"
            lblEmailStatus.Text = "Please enter your email address"
        End If
    End Sub

    Private Sub txtUsername_TextChanged(sender As Object, e As EventArgs)
        Dim username As String = txtUsername.Text.Trim()
        If username.Length < 4 Then
            lblUsernameStatus.ForeColor = Color.Orange
            lblUsernameStatus.Text = "Username too short (min 4 characters)"
        ElseIf Not Regex.IsMatch(username, "^[a-zA-Z0-9_]+$") Then
            lblUsernameStatus.ForeColor = Color.Orange
            lblUsernameStatus.Text = "Username must be alphanumeric/underscores only"
        ElseIf existingUsernames.Contains(username.ToLower()) Then
            lblUsernameStatus.ForeColor = Color.Red
            lblUsernameStatus.Text = "Username already taken"
        Else
            lblUsernameStatus.ForeColor = Color.LimeGreen
            lblUsernameStatus.Text = "Username is available!"
        End If
    End Sub

    Private Sub txtEmail_TextChanged(sender As Object, e As EventArgs)
        Dim email As String = txtEmail.Text.Trim()
        If Not Regex.IsMatch(email, "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$") Then
            lblEmailStatus.ForeColor = Color.Orange
            lblEmailStatus.Text = "Please enter a valid email address"
        ElseIf existingEmails.Contains(email.ToLower()) Then
            lblEmailStatus.ForeColor = Color.Red
            lblEmailStatus.Text = "Email already taken"
        Else
            lblEmailStatus.ForeColor = Color.LimeGreen
            lblEmailStatus.Text = "Email is available!"
        End If
    End Sub

    Private Sub txtPassword_TextChanged(sender As Object, e As EventArgs)
        Dim password As String = txtPassword.Text
        Dim strong = password.Length >= 8 AndAlso
                     Regex.IsMatch(password, "[A-Z]") AndAlso
                     Regex.IsMatch(password, "[a-z]") AndAlso
                     Regex.IsMatch(password, "\d")

        If strong Then
            lblPasswordStatus.ForeColor = Color.LimeGreen
            lblPasswordStatus.Text = "Password strength: Strong"
        Else
            lblPasswordStatus.ForeColor = Color.Orange
            lblPasswordStatus.Text = "Password must be 8+ chars, incl. upper, lower, digit"
        End If
    End Sub

    Private Sub btnRegister_Click(sender As Object, e As EventArgs)
        Dim username = txtUsername.Text.Trim()
        Dim email = txtEmail.Text.Trim()
        Dim password = txtPassword.Text
        Dim dob = dtpDob.Value

        If lblUsernameStatus.ForeColor <> Color.LimeGreen Then
            MessageBox.Show("Please fix your username before registering.")
            Return
        End If

        If lblEmailStatus.ForeColor <> Color.LimeGreen Then
            MessageBox.Show("Please fix your email before registering.")
            Return
        End If

        If lblPasswordStatus.ForeColor <> Color.LimeGreen Then
            MessageBox.Show("Please fix your password before registering.")
            Return
        End If

        ' Ensure DOB is at least 13 years old before confirming registration
        If dob > DateTime.Today.AddYears(-13) Then
            MessageBox.Show("You must be at least 13 years old to register.")
            Return
        End If

        ' Hash the password
        Dim hashedPassword As String = BCrypt.Net.BCrypt.HashPassword(password)

        ' Try to register the user with retry logic
        If RegisterUserWithRetry(username, email, hashedPassword, dob) Then
            MessageBox.Show("Account created successfully!")
            Me.Hide()
            Form1.Show()
        Else
            ' Generic message that doesn't expose database errors
            MessageBox.Show("Unable to complete registration at this time. Please try again later.")
        End If
    End Sub

    Private Function RegisterUserWithRetry(username As String, email As String, hashedPassword As String, dob As DateTime) As Boolean
        Dim connStr As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true"
        Dim insertQuery As String = "INSERT INTO users (username, password_hash, email, is_admin, date_of_birth) VALUES (@username, @password_hash, @email, FALSE, @dob)"
        Dim attemptCount As Integer = 0

        While attemptCount < MaxRetryAttempts
            attemptCount += 1

            Using conn As New NpgsqlConnection(connStr)
                Try
                    conn.Open()

                    ' Check for any existing username before inserting
                    Dim checkUsernameQuery As String = "SELECT COUNT(*) FROM users WHERE username = @username"
                    Dim checkUsernameCmd As New NpgsqlCommand(checkUsernameQuery, conn)
                    checkUsernameCmd.Parameters.AddWithValue("username", username)
                    Dim usernameCount As Integer = Convert.ToInt32(checkUsernameCmd.ExecuteScalar())

                    If usernameCount > 0 Then
                        MessageBox.Show("Username is already taken!")
                        Return False
                    End If

                    ' Check for any existing email before inserting
                    Dim checkEmailQuery As String = "SELECT COUNT(*) FROM users WHERE email = @email"
                    Dim checkEmailCmd As New NpgsqlCommand(checkEmailQuery, conn)
                    checkEmailCmd.Parameters.AddWithValue("email", email)
                    Dim emailCount As Integer = Convert.ToInt32(checkEmailCmd.ExecuteScalar())

                    If emailCount > 0 Then
                        MessageBox.Show("Email is already in use!")
                        Return False
                    End If

                    ' If no duplicates, insert the new user
                    Dim cmd As New NpgsqlCommand(insertQuery, conn)
                    cmd.Parameters.AddWithValue("username", username)
                    cmd.Parameters.AddWithValue("password_hash", hashedPassword)
                    cmd.Parameters.AddWithValue("email", email)
                    cmd.Parameters.AddWithValue("dob", dob)
                    cmd.ExecuteNonQuery()

                    ' Successfully registered
                    Return True

                Catch ex As Exception


                    If attemptCount < MaxRetryAttempts Then
                        ' Wait before retrying
                        System.Threading.Thread.Sleep(RetryDelayMs * attemptCount) ' Progressive backoff
                    End If
                End Try
            End Using
        End While

        ' If we get here, all attempts failed
        Return False
    End Function

    Private Sub btnBack_Click(sender As Object, e As EventArgs)
        Me.Hide()
        Form1.Show()
    End Sub

End Class
