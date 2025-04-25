Imports System.Windows.Forms

Public Class Form1

    Private WithEvents btnLogin As Button
    Private WithEvents btnSignUp As Button
    Private lblTitle As Label
    Private lblSubtitle As Label

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' === Form Appearance ===
        Me.Text = "Nutrify - Welcome"
        Me.Size = New Size(700, 500)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.BackColor = Color.FromArgb(28, 28, 30)

        ' === App Title Label ===
        lblTitle = New Label With {
            .Text = "Nutrify",
            .Font = New Font("Segoe UI", 36, FontStyle.Bold),
            .ForeColor = Color.LimeGreen,
            .AutoSize = True
        }
        lblTitle.Location = New Point((Me.ClientSize.Width - lblTitle.Width) \ 2, 60)
        Me.Controls.Add(lblTitle)
        lblTitle.Left = (Me.ClientSize.Width - lblTitle.Width) \ 2

        ' === Subtitle Label ===
        lblSubtitle = New Label With {
            .Text = "Your Fitness and Nutrition Tracker",
            .Font = New Font("Segoe UI", 16, FontStyle.Regular),
            .ForeColor = Color.LightGray,
            .AutoSize = True
        }
        lblSubtitle.Location = New Point((Me.ClientSize.Width - lblSubtitle.Width) \ 2, 130)
        Me.Controls.Add(lblSubtitle)
        lblSubtitle.Left = (Me.ClientSize.Width - lblSubtitle.Width) \ 2

        ' === Login Button ===
        btnLogin = New Button With {
            .Text = "Log In",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .BackColor = Color.FromArgb(45, 45, 48),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(250, 55),
            .Location = New Point((Me.ClientSize.Width - 250) \ 2, 220)
        }
        btnLogin.FlatAppearance.BorderSize = 0
        Me.Controls.Add(btnLogin)

        ' === Sign Up Button ===
        btnSignUp = New Button With {
            .Text = "Create Account",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .BackColor = Color.LimeGreen,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Size = New Size(250, 55),
            .Location = New Point((Me.ClientSize.Width - 250) \ 2, 300)
        }
        btnSignUp.FlatAppearance.BorderSize = 0
        Me.Controls.Add(btnSignUp)
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Me.Hide()
        UserLoginForm.Show()
    End Sub

    Private Sub btnSignUp_Click(sender As Object, e As EventArgs) Handles btnSignUp.Click
        Me.Hide()
        UserRegisterAccountForm.Show()
    End Sub

End Class
