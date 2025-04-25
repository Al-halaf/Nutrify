Imports System.Text.RegularExpressions
Imports Npgsql
Imports BCrypt.Net
Imports System.Threading

Public Class UserSettingsForm
    ' Connection string (same one used in other forms)
    Private ReadOnly connString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;" &
        "Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;" &
        "SSL Mode=Require;Trust Server Certificate=true"

    ' UI colors to match existing forms
    Private ReadOnly BackgroundColor As Color = Color.FromArgb(30, 30, 30)
    Private ReadOnly PrimaryColor As Color = Color.LimeGreen
    Private ReadOnly ErrorColor As Color = Color.Red
    Private ReadOnly ButtonColor As Color = Color.LimeGreen
    Private ReadOnly BackButtonColor As Color = Color.DimGray
    Private ReadOnly DeleteButtonColor As Color = Color.DarkRed

    ' Controls
    Private txtCurrentPassword As TextBox
    Private txtNewPassword As TextBox
    Private txtConfirmPassword As TextBox
    Private lblPasswordStatus As Label
    Private txtDeleteAccountPassword As TextBox

    ' Retry configuration
    Private Const MaxRetries As Integer = 3
    Private Const RetryDelayMs As Integer = 1000

    Private Sub UserSettingsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Settings"
        Me.Size = New Size(600, 680)
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = BackgroundColor

        InitializeUI()
    End Sub

    Private Sub InitializeUI()
        ' Title
        Dim lblTitle As New Label With {
            .Text = "Settings",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = PrimaryColor,
            .Location = New Point(240, 20),
            .AutoSize = True
        }
        Me.Controls.Add(lblTitle)

        ' Username display - using global CurrentUsername
        Dim lblUserInfo As New Label With {
            .Text = $"User: {CurrentUsername}",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(50, 70),
            .AutoSize = True
        }
        Me.Controls.Add(lblUserInfo)

        ' Section divider
        AddSectionDivider("Change Password", 120)

        ' Current Password
        Dim lblCurrentPassword As New Label With {
            .Text = "Current Password",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(50, 170),
            .AutoSize = True
        }
        Me.Controls.Add(lblCurrentPassword)

        txtCurrentPassword = New TextBox With {
            .Location = New Point(240, 170),
            .Size = New Size(300, 30),
            .Font = New Font("Segoe UI", 12),
            .PasswordChar = "●"c
        }
        Me.Controls.Add(txtCurrentPassword)

        ' New Password
        Dim lblNewPassword As New Label With {
            .Text = "New Password",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(50, 220),
            .AutoSize = True
        }
        Me.Controls.Add(lblNewPassword)

        txtNewPassword = New TextBox With {
            .Location = New Point(240, 220),
            .Size = New Size(300, 30),
            .Font = New Font("Segoe UI", 12),
            .PasswordChar = "●"c
        }
        AddHandler txtNewPassword.TextChanged, AddressOf txtNewPassword_TextChanged
        Me.Controls.Add(txtNewPassword)

        ' Confirm New Password
        Dim lblConfirmPassword As New Label With {
            .Text = "Confirm Password",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(50, 270),
            .AutoSize = True
        }
        Me.Controls.Add(lblConfirmPassword)

        txtConfirmPassword = New TextBox With {
            .Location = New Point(240, 270),
            .Size = New Size(300, 30),
            .Font = New Font("Segoe UI", 12),
            .PasswordChar = "●"c
        }
        AddHandler txtConfirmPassword.TextChanged, AddressOf txtConfirmPassword_TextChanged
        Me.Controls.Add(txtConfirmPassword)

        ' Password Validation Label
        lblPasswordStatus = New Label With {
            .Location = New Point(240, 305),
            .AutoSize = True,
            .ForeColor = Color.White
        }
        Me.Controls.Add(lblPasswordStatus)

        ' Update Password Button
        Dim btnUpdatePassword As New Button With {
            .Text = "Update Password",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(300, 40),
            .Location = New Point(150, 340)
        }
        AddHandler btnUpdatePassword.Click, AddressOf btnUpdatePassword_Click
        Me.Controls.Add(btnUpdatePassword)

        ' Section divider for Delete Account
        AddSectionDivider("Delete Account", 400)

        ' Warning Label
        Dim lblDeleteWarning As New Label With {
            .Text = "Warning: This action cannot be undone. All your data will be permanently deleted.",
            .Font = New Font("Segoe UI", 9, FontStyle.Italic),
            .ForeColor = ErrorColor,
            .Location = New Point(50, 440),
            .AutoSize = True
        }
        Me.Controls.Add(lblDeleteWarning)

        ' Password for account deletion
        Dim lblDeletePassword As New Label With {
            .Text = "Enter Password",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(50, 480),
            .AutoSize = True
        }
        Me.Controls.Add(lblDeletePassword)

        txtDeleteAccountPassword = New TextBox With {
            .Location = New Point(240, 480),
            .Size = New Size(300, 30),
            .Font = New Font("Segoe UI", 12),
            .PasswordChar = "●"c
        }
        Me.Controls.Add(txtDeleteAccountPassword)

        ' Delete Account Button
        Dim btnDeleteAccount As New Button With {
            .Text = "Delete My Account",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .BackColor = DeleteButtonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(300, 40),
            .Location = New Point(150, 530)
        }
        AddHandler btnDeleteAccount.Click, AddressOf btnDeleteAccount_Click
        Me.Controls.Add(btnDeleteAccount)

        ' Back Button
        Dim btnBack As New Button With {
            .Text = "Back to Dashboard",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = BackButtonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(150, 30),
            .Location = New Point(50, 590)
        }
        AddHandler btnBack.Click, AddressOf btnBack_Click
        Me.Controls.Add(btnBack)
    End Sub

    Private Sub AddSectionDivider(sectionTitle As String, yPosition As Integer)
        ' Section header
        Dim lblSection As New Label With {
            .Text = sectionTitle,
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .ForeColor = PrimaryColor,
            .Location = New Point(50, yPosition),
            .AutoSize = True
        }
        Me.Controls.Add(lblSection)

        ' Horizontal line
        Dim linePanel As New Panel With {
            .BackColor = Color.Gray,
            .Location = New Point(50, yPosition + 30),
            .Size = New Size(500, 2)
        }
        Me.Controls.Add(linePanel)
    End Sub

    Private Sub txtNewPassword_TextChanged(sender As Object, e As EventArgs)
        ValidatePassword()
    End Sub

    Private Sub txtConfirmPassword_TextChanged(sender As Object, e As EventArgs)
        ValidatePassword()
    End Sub

    Private Sub ValidatePassword()
        Dim password As String = txtNewPassword.Text
        Dim confirmPassword As String = txtConfirmPassword.Text

        ' Check if passwords match
        If txtConfirmPassword.Text.Length > 0 AndAlso password <> confirmPassword Then
            lblPasswordStatus.ForeColor = ErrorColor
            lblPasswordStatus.Text = "Passwords do not match"
            Return
        End If

        ' Check password strength as per registration requirements
        Dim strong = password.Length >= 8 AndAlso
                     Regex.IsMatch(password, "[A-Z]") AndAlso
                     Regex.IsMatch(password, "[a-z]") AndAlso
                     Regex.IsMatch(password, "\d")

        If password.Length = 0 Then
            lblPasswordStatus.Text = ""
        ElseIf strong Then
            lblPasswordStatus.ForeColor = PrimaryColor
            lblPasswordStatus.Text = "Password strength: Strong"
        Else
            lblPasswordStatus.ForeColor = Color.Orange
            lblPasswordStatus.Text = "Password must be 8+ chars, incl. upper, lower, digit"
        End If
    End Sub

    Private Sub btnUpdatePassword_Click(sender As Object, e As EventArgs)
        Dim currentPassword As String = txtCurrentPassword.Text
        Dim newPassword As String = txtNewPassword.Text
        Dim confirmPassword As String = txtConfirmPassword.Text

        ' Validate inputs
        If String.IsNullOrEmpty(currentPassword) Then
            MessageBox.Show("Please enter your current password.")
            Return
        End If

        If String.IsNullOrEmpty(newPassword) Then
            MessageBox.Show("Please enter a new password.")
            Return
        End If

        If newPassword <> confirmPassword Then
            MessageBox.Show("New passwords do not match.")
            Return
        End If

        ' Validate password strength
        Dim isStrongPassword As Boolean = newPassword.Length >= 8 AndAlso
                                         Regex.IsMatch(newPassword, "[A-Z]") AndAlso
                                         Regex.IsMatch(newPassword, "[a-z]") AndAlso
                                         Regex.IsMatch(newPassword, "\d")

        If Not isStrongPassword Then
            MessageBox.Show("Password must be at least 8 characters and include uppercase, lowercase, and a number.")
            Return
        End If

        ' Verify current password and update if correct
        If VerifyAndUpdatePassword(currentPassword, newPassword) Then
            MessageBox.Show("Password updated successfully!")
            ClearPasswordFields()
        Else
            MessageBox.Show("Current password is incorrect.")
        End If
    End Sub

    Private Function VerifyAndUpdatePassword(currentPassword As String, newPassword As String) As Boolean
        ' Set up for retry logic
        Dim retryCount As Integer = 0
        Dim success As Boolean = False

        While retryCount < MaxRetries AndAlso Not success
            Try
                Using conn As New NpgsqlConnection(connString)
                    conn.Open()

                    ' Get current password hash - using global CurrUserID
                    Dim queryGetHash As String = "SELECT password_hash FROM users WHERE user_id = @userId"
                    Dim cmdGetHash As New NpgsqlCommand(queryGetHash, conn)
                    cmdGetHash.Parameters.AddWithValue("userId", CurrUserID)
                    Dim currentHash As String = cmdGetHash.ExecuteScalar().ToString()

                    ' Verify current password
                    If Not BCrypt.Net.BCrypt.Verify(currentPassword, currentHash) Then
                        Return False
                    End If

                    ' Hash and update new password
                    Dim newHash As String = BCrypt.Net.BCrypt.HashPassword(newPassword)
                    Dim queryUpdate As String = "UPDATE users SET password_hash = @newHash WHERE user_id = @userId"
                    Dim cmdUpdate As New NpgsqlCommand(queryUpdate, conn)
                    cmdUpdate.Parameters.AddWithValue("newHash", newHash)
                    cmdUpdate.Parameters.AddWithValue("userId", CurrUserID)
                    cmdUpdate.ExecuteNonQuery()

                    success = True
                    Return True
                End Using
            Catch ex As Exception
                ' Increment retry counter
                retryCount += 1

                ' If we haven't reached max retries yet, wait and try again
                If retryCount < MaxRetries Then
                    Thread.Sleep(RetryDelayMs)
                End If
            End Try
        End While

        ' If we get here and success is still false, all retries failed
        Return success
    End Function

    Private Sub btnDeleteAccount_Click(sender As Object, e As EventArgs)
        Dim password As String = txtDeleteAccountPassword.Text

        If String.IsNullOrEmpty(password) Then
            MessageBox.Show("Please enter your password to confirm account deletion.")
            Return
        End If

        ' Confirm with user
        Dim response As DialogResult = MessageBox.Show(
            "Are you sure you want to delete your account? This action cannot be undone.",
            "Confirm Account Deletion",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning)

        If response = DialogResult.Yes Then
            If VerifyAndDeleteAccount(password) Then
                MessageBox.Show("Your account has been successfully deleted.")
                ' Return to login screen
                Me.Hide()
                Form1.Show()
            Else
                MessageBox.Show("Password is incorrect. Account deletion canceled.")
            End If
        End If
    End Sub

    Private Function VerifyAndDeleteAccount(password As String) As Boolean
        ' Set up for retry logic
        Dim retryCount As Integer = 0
        Dim success As Boolean = False

        While retryCount < MaxRetries AndAlso Not success
            Try
                Using conn As New NpgsqlConnection(connString)
                    conn.Open()

                    ' Get current password hash - using global CurrUserID
                    Dim queryGetHash As String = "SELECT password_hash FROM users WHERE user_id = @userId"
                    Dim cmdGetHash As New NpgsqlCommand(queryGetHash, conn)
                    cmdGetHash.Parameters.AddWithValue("userId", CurrUserID)
                    Dim currentHash As String = cmdGetHash.ExecuteScalar().ToString()

                    ' Verify password
                    If Not BCrypt.Net.BCrypt.Verify(password, currentHash) Then
                        Return False
                    End If

                    ' Begin a transaction for all delete operations
                    Using transaction As NpgsqlTransaction = conn.BeginTransaction()
                        Try
                            ' Delete related records first based on the DB schema

                            ' 1. Delete from User_Logins
                            Dim queryDeleteLogins As String = "DELETE FROM User_Logins WHERE user_id = @userId"
                            Using cmdDeleteLogins As New NpgsqlCommand(queryDeleteLogins, conn, transaction)
                                cmdDeleteLogins.Parameters.AddWithValue("userId", CurrUserID)
                                cmdDeleteLogins.ExecuteNonQuery()
                            End Using

                            ' 2. Delete from Points
                            Dim queryDeletePoints As String = "DELETE FROM Points WHERE user_id = @userId"
                            Using cmdDeletePoints As New NpgsqlCommand(queryDeletePoints, conn, transaction)
                                cmdDeletePoints.Parameters.AddWithValue("userId", CurrUserID)
                                cmdDeletePoints.ExecuteNonQuery()
                            End Using

                            ' 3. Delete from User_Foods
                            Dim queryDeleteFoods As String = "DELETE FROM User_Foods WHERE user_id = @userId"
                            Using cmdDeleteFoods As New NpgsqlCommand(queryDeleteFoods, conn, transaction)
                                cmdDeleteFoods.Parameters.AddWithValue("userId", CurrUserID)
                                cmdDeleteFoods.ExecuteNonQuery()
                            End Using

                            ' 4. Delete from User_Workouts
                            Dim queryDeleteWorkouts As String = "DELETE FROM User_Workouts WHERE user_id = @userId"
                            Using cmdDeleteWorkouts As New NpgsqlCommand(queryDeleteWorkouts, conn, transaction)
                                cmdDeleteWorkouts.Parameters.AddWithValue("userId", CurrUserID)
                                cmdDeleteWorkouts.ExecuteNonQuery()
                            End Using

                            ' Finally delete the user account
                            Dim queryDeleteUser As String = "DELETE FROM Users WHERE user_id = @userId"
                            Using cmdDeleteUser As New NpgsqlCommand(queryDeleteUser, conn, transaction)
                                cmdDeleteUser.Parameters.AddWithValue("userId", CurrUserID)
                                cmdDeleteUser.ExecuteNonQuery()
                            End Using

                            ' Commit all deletions
                            transaction.Commit()
                            success = True
                            Return True
                        Catch ex As Exception
                            ' Rollback all changes if any error occurs
                            transaction.Rollback()

                            ' If we're on the last retry, show message box
                            If retryCount = (MaxRetries - 1) Then
                                ' Don't show any message, just return false
                                Return False
                            End If

                            ' Increment retry counter and try again
                            retryCount += 1
                            Thread.Sleep(RetryDelayMs)
                        End Try
                    End Using
                End Using
            Catch ex As Exception
                ' Increment retry counter
                retryCount += 1

                ' If we haven't reached max retries yet, wait and try again
                If retryCount < MaxRetries Then
                    Thread.Sleep(RetryDelayMs)
                End If
            End Try
        End While

        ' If we get here and success is still false, all retries failed
        Return success
    End Function

    Private Sub ClearPasswordFields()
        txtCurrentPassword.Clear()
        txtNewPassword.Clear()
        txtConfirmPassword.Clear()
        lblPasswordStatus.Text = ""
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs)
        Me.Hide()
        UserDashboard.Show()
    End Sub

    Private Sub UserSettingsForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            Me.Hide()
            UserDashboard.Show()
        End If
    End Sub
End Class