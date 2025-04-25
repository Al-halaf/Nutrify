Imports Npgsql
Imports BCrypt.Net

Public Class UserLoginForm
    Private txtUsername As TextBox
    Private txtPassword As TextBox
    Private lblStatus As Label

    Private Sub UserLoginForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Login"
        Me.Size = New Size(650, 600)  ' Increased form size
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(30, 30, 30)
        Logintrue = False
        MealLogtrue = False
        WorkoutLogtrue = False
        Loginmessage = False

        InitializeUI()
    End Sub

    Private Sub InitializeUI()
        ' Title
        Dim lblTitle As New Label With {
            .Text = "Login to Nutrify",
            .Font = New Font("Segoe UI", 24, FontStyle.Bold),  ' Increased font size
            .ForeColor = Color.LimeGreen,
            .Location = New Point(200, 50),  ' Adjusted position
            .AutoSize = True
        }
        Me.Controls.Add(lblTitle)

        ' Username Label
        Dim lblUsername As New Label With {
            .Text = "Username",
            .Font = New Font("Segoe UI", 14),  ' Increased font size
            .ForeColor = Color.Gray,
            .Location = New Point(100, 150),  ' Adjusted position
            .AutoSize = True
        }
        Me.Controls.Add(lblUsername)

        txtUsername = New TextBox With {
            .Font = New Font("Segoe UI", 14),  ' Increased font size
            .Location = New Point(100, 180),  ' Adjusted position
            .Size = New Size(450, 35)  ' Increased size
        }
        Me.Controls.Add(txtUsername)

        ' Password Label
        Dim lblPassword As New Label With {
            .Text = "Password",
            .Font = New Font("Segoe UI", 14),  ' Increased font size
            .ForeColor = Color.Gray,
            .Location = New Point(100, 230),  ' Adjusted position
            .AutoSize = True
        }
        Me.Controls.Add(lblPassword)

        txtPassword = New TextBox With {
            .Font = New Font("Segoe UI", 14),  ' Increased font size
            .Location = New Point(100, 260),  ' Adjusted position
            .Size = New Size(450, 35),  ' Increased size
            .PasswordChar = "●"c
        }
        Me.Controls.Add(txtPassword)

        ' Status Label
        lblStatus = New Label With {
            .Font = New Font("Segoe UI", 12),  ' Added font
            .ForeColor = Color.Red,
            .Location = New Point(100, 305),  ' Adjusted position
            .AutoSize = True
        }
        Me.Controls.Add(lblStatus)

        ' Login Button
        Dim btnLogin As New Button With {
            .Text = "Login",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),  ' Increased font size
            .BackColor = Color.LimeGreen,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(450, 50),  ' Increased size
            .Location = New Point(100, 340)  ' Adjusted position
        }
        AddHandler btnLogin.Click, AddressOf btnLogin_Click
        Me.Controls.Add(btnLogin)

        ' Register Section
        Dim lblRegister As New Label With {
            .Text = "Don't have an account?",
            .Font = New Font("Segoe UI", 12),  ' Increased font size
            .ForeColor = Color.Gray,
            .Location = New Point(100, 420),  ' Adjusted position
            .AutoSize = True
        }
        Me.Controls.Add(lblRegister)

        ' Register Button
        Dim btnRegister As New Button With {
            .Text = "Register",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),  ' Increased font size
            .BackColor = Color.DodgerBlue,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(200, 40),  ' Increased size
            .Location = New Point(350, 410)  ' Adjusted position
        }
        AddHandler btnRegister.Click, AddressOf btnRegister_Click
        Me.Controls.Add(btnRegister)

        ' Back Section
        Dim lblBack As New Label With {
            .Text = "Return to start?",
            .Font = New Font("Segoe UI", 12),  ' Increased font size
            .ForeColor = Color.Gray,
            .Location = New Point(100, 480),  ' Adjusted position
            .AutoSize = True
        }
        Me.Controls.Add(lblBack)

        ' Back Button
        Dim btnBack As New Button With {
            .Text = "Back",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),  ' Increased font size
            .BackColor = Color.DarkGray,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(200, 40),  ' Increased size
            .Location = New Point(350, 470)  ' Adjusted position
        }
        AddHandler btnBack.Click, AddressOf btnBack_Click
        Me.Controls.Add(btnBack)
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs)
        ' Login logic remains unchanged
        Dim username = txtUsername.Text.Trim()
        Dim password = txtPassword.Text
        CurrentUsername = username
        Logintrue = False ' Reset first

        If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
            lblStatus.Text = "Please enter both username and password"
            Return
        End If

        Dim connStr As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true"
        Dim query As String = "SELECT user_id, password_hash, is_admin FROM users WHERE username = @username"

        Using conn As New Npgsql.NpgsqlConnection(connStr)
            Try
                conn.Open()
                Dim cmd As New Npgsql.NpgsqlCommand(query, conn)
                cmd.Parameters.AddWithValue("username", username)

                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Dim userId = reader.GetInt32(0)
                        Dim dbHash = reader.GetString(1)
                        Dim isAdmin = reader.GetBoolean(2)


                        If BCrypt.Net.BCrypt.Verify(password, dbHash) Then

                            reader.Close() ' Close reader before executing more commands

                            ' Insert login record
                            Dim insertLoginCmd As New Npgsql.NpgsqlCommand("INSERT INTO User_Logins (user_id) VALUES (@user_id)", conn)
                            insertLoginCmd.Parameters.AddWithValue("user_id", userId)
                            insertLoginCmd.ExecuteNonQuery()
                            CurrUserID = userId

                            ' Check if this is the first login of the day
                            Dim checkCmd As New Npgsql.NpgsqlCommand("SELECT COUNT(*) FROM User_Logins WHERE user_id = @user_id AND login_date = CURRENT_DATE", conn)
                            checkCmd.Parameters.AddWithValue("user_id", userId)
                            Dim loginCount As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                            If loginCount = 1 Then
                                ' First login today → award points
                                Dim insertPointsCmd As New Npgsql.NpgsqlCommand("INSERT INTO Points (user_id, points, date_recorded) VALUES (@user_id, 50, CURRENT_DATE)", conn)
                                insertPointsCmd.Parameters.AddWithValue("user_id", userId)
                                insertPointsCmd.ExecuteNonQuery()
                                Logintrue = True ' Only set if points awarded
                            End If

                            lblStatus.ForeColor = Color.LimeGreen
                            lblStatus.Text = "Login successful!"

                            Me.Hide()
                            If isAdmin Then
                                Loginmessage = True
                                AdminDashboard.Show()
                            Else
                                Loginmessage = True
                                UserDashboard.Show()
                            End If
                        Else
                            lblStatus.Text = "Incorrect password"
                        End If
                    Else
                        lblStatus.Text = "Username not found"
                    End If
                End Using
            Catch ex As Exception
                lblStatus.Text = "Login error: " & ex.Message
            End Try
        End Using
    End Sub

    Private Sub btnRegister_Click(sender As Object, e As EventArgs)
        Dim registerForm As New UserRegisterAccountForm()
        registerForm.Show()
        Me.Hide()
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs)
        Dim form1 As New Form1()
        form1.Show()
        Me.Hide()
    End Sub
End Class