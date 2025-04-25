Public Class CreateAdminForm

    Private Sub CreateAdminForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            ' Form settings
            Me.Text = "Create New Admin"
            Me.Size = New Size(400, 300)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.BackColor = Color.FromArgb(30, 30, 30)

            ' Add controls (TextBoxes for username, email, password)
            Dim txtUsername As New TextBox With {
            .Font = New Font("Segoe UI", 11),
            .Location = New Point(20, 30),
            .Width = 350
        }
            Me.Controls.Add(txtUsername)

            Dim txtEmail As New TextBox With {
            .Font = New Font("Segoe UI", 11),
            .Location = New Point(20, 80),
            .Width = 350
        }
            Me.Controls.Add(txtEmail)

            Dim txtPassword As New TextBox With {
            .Font = New Font("Segoe UI", 11),
            .Location = New Point(20, 130),
            .Width = 350,
            .PasswordChar = "*"
        }
            Me.Controls.Add(txtPassword)

            ' Create Admin Button
            Dim btnCreate As New Button With {
            .Text = "Create Admin",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .BackColor = Color.LimeGreen,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(20, 180),
            .Size = New Size(200, 40)
        }
            AddHandler btnCreate.Click, AddressOf btnCreate_Click
            Me.Controls.Add(btnCreate)
        End Sub

        Private Sub btnCreate_Click(sender As Object, e As EventArgs)
            ' Logic to create new admin
            ' Validate input and insert the new admin into the database
            MessageBox.Show("New admin account created!")
        End Sub
    End Class

