Imports System.Text.RegularExpressions
Imports Npgsql
Imports BCrypt.Net
Imports System.Threading

Public Class AdminSettingsForm
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

    ' Controls
    Private txtCurrentPassword As TextBox
    Private txtNewPassword As TextBox
    Private txtConfirmPassword As TextBox
    Private lblPasswordStatus As Label
    Private lblPasswordStrengthMeter As Label
    Private pnlPasswordStrength As Panel

    ' Password strength indicators
    Private lblLengthCheck As Label
    Private lblUpperCheck As Label
    Private lblLowerCheck As Label
    Private lblNumberCheck As Label
    Private lblSpecialCheck As Label
    Private lblNoSequenceCheck As Label

    ' Retry configuration
    Private Const MaxRetries As Integer = 3
    Private Const RetryDelayMs As Integer = 1000

    Private Sub AdminSettingsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Admin Settings"
        Me.Size = New Size(700, 680)
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = BackgroundColor

        InitializeUI()
    End Sub

    Private Sub InitializeUI()
        ' Title
        Dim lblTitle As New Label With {
            .Text = "Admin Settings",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = PrimaryColor,
            .Location = New Point(240, 20),
            .AutoSize = True
        }
        Me.Controls.Add(lblTitle)

        ' Username display - using global CurrentUsername
        Dim lblUserInfo As New Label With {
            .Text = $"Admin: {CurrentUsername}",
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

        ' Password Validation Label
        lblPasswordStatus = New Label With {
            .Location = New Point(240, 255),
            .AutoSize = True,
            .ForeColor = Color.White
        }
        Me.Controls.Add(lblPasswordStatus)

        ' Password Strength Meter
        Dim lblPasswordStrengthText As New Label With {
            .Text = "Password Strength:",
            .Font = New Font("Segoe UI", 10),
            .ForeColor = Color.White,
            .Location = New Point(240, 280),
            .AutoSize = True
        }
        Me.Controls.Add(lblPasswordStrengthText)

        ' Strength meter background
        Dim pnlStrengthBackground As New Panel With {
            .Location = New Point(350, 280),
            .Size = New Size(190, 20),
            .BackColor = Color.FromArgb(60, 60, 60)
        }
        Me.Controls.Add(pnlStrengthBackground)

        ' Strength meter fill
        pnlPasswordStrength = New Panel With {
            .Location = New Point(350, 280),
            .Size = New Size(0, 20),
            .BackColor = Color.Red
        }
        Me.Controls.Add(pnlPasswordStrength)

        ' Strength meter label
        lblPasswordStrengthMeter = New Label With {
            .Text = "Weak",
            .Font = New Font("Segoe UI", 10),
            .ForeColor = Color.White,
            .Location = New Point(545, 280),
            .AutoSize = True
        }
        Me.Controls.Add(lblPasswordStrengthMeter)

        ' Password Validation Criteria
        Dim lblValidationTitle As New Label With {
            .Text = "Password Requirements:",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(240, 310),
            .AutoSize = True
        }
        Me.Controls.Add(lblValidationTitle)

        ' Criteria list
        lblLengthCheck = CreateCriteriaLabel("At least 12 characters", 335)
        lblUpperCheck = CreateCriteriaLabel("At least 1 uppercase letter", 355)
        lblLowerCheck = CreateCriteriaLabel("At least 1 lowercase letter", 375)
        lblNumberCheck = CreateCriteriaLabel("At least 1 number", 395)
        lblSpecialCheck = CreateCriteriaLabel("At least 1 special character", 415)
        lblNoSequenceCheck = CreateCriteriaLabel("No common sequences/patterns", 435)

        ' Confirm New Password
        Dim lblConfirmPassword As New Label With {
            .Text = "Confirm Password",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(50, 470),
            .AutoSize = True
        }
        Me.Controls.Add(lblConfirmPassword)

        txtConfirmPassword = New TextBox With {
            .Location = New Point(240, 470),
            .Size = New Size(300, 30),
            .Font = New Font("Segoe UI", 12),
            .PasswordChar = "●"c
        }
        AddHandler txtConfirmPassword.TextChanged, AddressOf txtConfirmPassword_TextChanged
        Me.Controls.Add(txtConfirmPassword)

        ' Update Password Button
        Dim btnUpdatePassword As New Button With {
            .Text = "Update Password",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(300, 40),
            .Location = New Point(150, 530)
        }
        AddHandler btnUpdatePassword.Click, AddressOf btnUpdatePassword_Click
        Me.Controls.Add(btnUpdatePassword)

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

    Private Function CreateCriteriaLabel(text As String, yPos As Integer) As Label
        Dim lbl As New Label With {
            .Text = "❌ " & text,
            .Font = New Font("Segoe UI", 9),
            .ForeColor = Color.LightGray,
            .Location = New Point(240, yPos),
            .AutoSize = True
        }
        Me.Controls.Add(lbl)
        Return lbl
    End Function

    Private Sub AddSectionDivider(sectionTitle As String, yPosition As Integer)
        Dim lblSection As New Label With {
            .Text = sectionTitle,
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .ForeColor = PrimaryColor,
            .Location = New Point(50, yPosition),
            .AutoSize = True
        }
        Me.Controls.Add(lblSection)

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
        If txtConfirmPassword.Text.Length > 0 AndAlso txtNewPassword.Text <> txtConfirmPassword.Text Then
            lblPasswordStatus.ForeColor = ErrorColor
            lblPasswordStatus.Text = "Passwords do not match"
        ElseIf txtConfirmPassword.Text.Length > 0 Then
            lblPasswordStatus.ForeColor = PrimaryColor
            lblPasswordStatus.Text = "Passwords match"
        End If
    End Sub

    Private Sub ValidatePassword()
        Dim password As String = txtNewPassword.Text

        Dim hasLength As Boolean = password.Length >= 12
        Dim hasUppercase As Boolean = Regex.IsMatch(password, "[A-Z]")
        Dim hasLowercase As Boolean = Regex.IsMatch(password, "[a-z]")
        Dim hasNumber As Boolean = Regex.IsMatch(password, "\d")
        Dim hasSpecial As Boolean = Regex.IsMatch(password, "[^a-zA-Z0-9]")
        Dim hasSequence As Boolean = ContainsCommonSequences(password)
        Dim noSequences As Boolean = Not hasSequence

        UpdateCriteriaLabel(lblLengthCheck, hasLength, "At least 12 characters")
        UpdateCriteriaLabel(lblUpperCheck, hasUppercase, "At least 1 uppercase letter")
        UpdateCriteriaLabel(lblLowerCheck, hasLowercase, "At least 1 lowercase letter")
        UpdateCriteriaLabel(lblNumberCheck, hasNumber, "At least 1 number")
        UpdateCriteriaLabel(lblSpecialCheck, hasSpecial, "At least 1 special character")
        UpdateCriteriaLabel(lblNoSequenceCheck, noSequences, "No common sequences/patterns")

        Dim strengthScore As Integer = 0
        If hasLength Then strengthScore += 1
        If hasUppercase Then strengthScore += 1
        If hasLowercase Then strengthScore += 1
        If hasNumber Then strengthScore += 1
        If hasSpecial Then strengthScore += 1
        If noSequences Then strengthScore += 1

        Dim strengthCategory As String
        Dim strengthColor As Color

        Select Case strengthScore
            Case 0, 1
                strengthCategory = "Very Weak"
                strengthColor = Color.Red
            Case 2
                strengthCategory = "Weak"
                strengthColor = Color.OrangeRed
            Case 3, 4
                strengthCategory = "Moderate"
                strengthColor = Color.Orange
            Case 5
                strengthCategory = "Strong"
                strengthColor = Color.Yellow
            Case 6
                strengthCategory = "Very Strong"
                strengthColor = Color.LimeGreen
            Case Else
                strengthCategory = "Unknown"
                strengthColor = Color.Gray
        End Select

        Dim meterWidth As Integer = CInt(190 * (strengthScore / 6))
        pnlPasswordStrength.Size = New Size(meterWidth, 20)
        pnlPasswordStrength.BackColor = strengthColor
        lblPasswordStrengthMeter.Text = strengthCategory

        Dim meetsAllCriteria As Boolean = strengthScore = 6

        If password.Length = 0 Then
            lblPasswordStatus.Text = ""
        ElseIf meetsAllCriteria Then
            lblPasswordStatus.ForeColor = PrimaryColor
            lblPasswordStatus.Text = "Password strength: " & strengthCategory
        Else
            lblPasswordStatus.ForeColor = Color.Orange
            lblPasswordStatus.Text = "All requirements must be met"
        End If
    End Sub

    Private Sub UpdateCriteriaLabel(label As Label, condition As Boolean, text As String)
        If condition Then
            label.Text = "✓ " & text
            label.ForeColor = PrimaryColor
        Else
            label.Text = "❌ " & text
            label.ForeColor = Color.LightGray
        End If
    End Sub

    Private Function ContainsCommonSequences(password As String) As Boolean
        Dim sequences As String() = {"qwerty", "asdfgh", "zxcvbn", "qwertz", "123456", "abcdef"}

        For Each seq As String In sequences
            If password.ToLower().Contains(seq) OrElse password.ToLower().Contains(StrReverse(seq)) Then
                Return True
            End If
        Next

        For i As Integer = 0 To password.Length - 3
            If password(i) = password(i + 1) AndAlso password(i) = password(i + 2) Then
                Return True
            End If
        Next

        For i As Integer = 0 To password.Length - 3
            If Char.IsLetter(password(i)) AndAlso Char.IsLetter(password(i + 1)) AndAlso Char.IsLetter(password(i + 2)) Then
                If Asc(Char.ToLower(password(i + 1))) = Asc(Char.ToLower(password(i))) + 1 AndAlso
                   Asc(Char.ToLower(password(i + 2))) = Asc(Char.ToLower(password(i + 1))) + 1 Then
                    Return True
                End If
            End If

            If Char.IsDigit(password(i)) AndAlso Char.IsDigit(password(i + 1)) AndAlso Char.IsDigit(password(i + 2)) Then
                If Asc(password(i + 1)) = Asc(password(i)) + 1 AndAlso
                   Asc(password(i + 2)) = Asc(password(i + 1)) + 1 Then
                    Return True
                End If
            End If
        Next

        Return False
    End Function

    Private Sub btnUpdatePassword_Click(sender As Object, e As EventArgs)
        Dim currentPassword As String = txtCurrentPassword.Text
        Dim newPassword As String = txtNewPassword.Text
        Dim confirmPassword As String = txtConfirmPassword.Text

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

        Dim hasLength As Boolean = newPassword.Length >= 12
        Dim hasUppercase As Boolean = Regex.IsMatch(newPassword, "[A-Z]")
        Dim hasLowercase As Boolean = Regex.IsMatch(newPassword, "[a-z]")
        Dim hasNumber As Boolean = Regex.IsMatch(newPassword, "\d")
        Dim hasSpecial As Boolean = Regex.IsMatch(newPassword, "[^a-zA-Z0-9]")
        Dim noSequences As Boolean = Not ContainsCommonSequences(newPassword)

        If Not (hasLength AndAlso hasUppercase AndAlso hasLowercase AndAlso hasNumber AndAlso hasSpecial AndAlso noSequences) Then
            MessageBox.Show("Your password must meet ALL security requirements for administrator accounts.")
            Return
        End If

        If VerifyAndUpdatePassword(currentPassword, newPassword) Then
            MessageBox.Show("Password updated successfully!")
            ClearPasswordFields()
        Else
            MessageBox.Show("Current password is incorrect.")
        End If
    End Sub

    Private Function VerifyAndUpdatePassword(currentPassword As String, newPassword As String) As Boolean
        Dim retryCount As Integer = 0
        Dim success As Boolean = False

        While retryCount < MaxRetries AndAlso Not success
            Try
                Using conn As New NpgsqlConnection(connString)
                    conn.Open()

                    Dim queryGetHash As String = "SELECT password_hash FROM users WHERE user_id = @userId"
                    Dim cmdGetHash As New NpgsqlCommand(queryGetHash, conn)
                    cmdGetHash.Parameters.AddWithValue("userId", CurrUserID)

                    Dim currentHash As String = Nothing
                    Dim result = cmdGetHash.ExecuteScalar()

                    If result IsNot Nothing Then
                        currentHash = result.ToString()
                    Else
                        Thread.Sleep(RetryDelayMs)
                        retryCount += 1
                        Continue While
                    End If

                    If Not BCrypt.Net.BCrypt.Verify(currentPassword, currentHash) Then
                        Return False
                    End If

                    Dim newHash As String = BCrypt.Net.BCrypt.HashPassword(newPassword)

                    Using transaction As NpgsqlTransaction = conn.BeginTransaction()
                        Try
                            Dim queryUpdate As String = "UPDATE users SET password_hash = @newHash WHERE user_id = @userId"
                            Dim cmdUpdate As New NpgsqlCommand(queryUpdate, conn, transaction)
                            cmdUpdate.Parameters.AddWithValue("newHash", newHash)
                            cmdUpdate.Parameters.AddWithValue("userId", CurrUserID)
                            Dim rowsAffected = cmdUpdate.ExecuteNonQuery()

                            If rowsAffected <= 0 Then
                                transaction.Rollback()
                                Thread.Sleep(RetryDelayMs)
                                retryCount += 1
                                Continue While
                            End If

                            Dim queryLog As String = "INSERT INTO Admin_Logs (user_id, action_type, action_details, timestamp) " &
                                                   "VALUES (@userId, 'password_change', 'Admin password changed', CURRENT_TIMESTAMP)"
                            Dim cmdLog As New NpgsqlCommand(queryLog, conn, transaction)
                            cmdLog.Parameters.AddWithValue("userId", CurrUserID)
                            cmdLog.ExecuteNonQuery()

                            transaction.Commit()
                            success = True
                        Catch ex As Exception
                            transaction.Rollback()
                            retryCount += 1

                            If retryCount < MaxRetries Then
                                Thread.Sleep(RetryDelayMs)
                            End If
                        End Try
                    End Using
                End Using
            Catch ex As Exception
                retryCount += 1

                If retryCount < MaxRetries Then
                    Thread.Sleep(RetryDelayMs)
                End If
            End Try
        End While

        Return success
    End Function

    Private Sub ClearPasswordFields()
        txtCurrentPassword.Clear()
        txtNewPassword.Clear()
        txtConfirmPassword.Clear()
        lblPasswordStatus.Text = ""

        UpdateCriteriaLabel(lblLengthCheck, False, "At least 12 characters")
        UpdateCriteriaLabel(lblUpperCheck, False, "At least 1 uppercase letter")
        UpdateCriteriaLabel(lblLowerCheck, False, "At least 1 lowercase letter")
        UpdateCriteriaLabel(lblNumberCheck, False, "At least 1 number")
        UpdateCriteriaLabel(lblSpecialCheck, False, "At least 1 special character")
        UpdateCriteriaLabel(lblNoSequenceCheck, False, "No common sequences/patterns")

        pnlPasswordStrength.Size = New Size(0, 20)
        lblPasswordStrengthMeter.Text = "Weak"
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs)
        Me.Hide()
        AdminDashboard.Show()
    End Sub

    Private Sub AdminSettingsForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            Me.Hide()
            AdminDashboard.Show()
        End If
    End Sub
End Class