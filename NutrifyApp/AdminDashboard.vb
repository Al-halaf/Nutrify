Imports System.Windows.Forms

Public Class AdminDashboard
    Private Sub AdminDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Form settings
        Me.Text = "Admin Dashboard"
        Me.Size = New Size(800, 600)
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(30, 30, 30) ' Dark background color

        ' Add controls
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        ' Title Label
        Dim lblTitle As New Label With {
            .Text = "Admin Dashboard",
            .Font = New Font("Segoe UI", 24, FontStyle.Bold),
            .ForeColor = Color.LimeGreen,
            .Location = New Point((Me.ClientSize.Width - 300) / 2, 20), ' Center the title
            .AutoSize = True
        }
        Me.Controls.Add(lblTitle)

        ' Buttons for managing workouts, foods, and creating new admin
        Dim btnManageWorkouts As New Button With {
            .Text = "Manage Workouts",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .BackColor = Color.LimeGreen,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point((Me.ClientSize.Width - 300) / 2, 100),
            .Size = New Size(300, 80) ' Larger button size
        }
        AddHandler btnManageWorkouts.Click, AddressOf btnManageWorkouts_Click
        Me.Controls.Add(btnManageWorkouts)

        Dim btnManageFoods As New Button With {
            .Text = "Manage Foods",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .BackColor = Color.LimeGreen,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point((Me.ClientSize.Width - 300) / 2, 200),
            .Size = New Size(300, 80) ' Larger button size
        }
        AddHandler btnManageFoods.Click, AddressOf btnManageFoods_Click
        Me.Controls.Add(btnManageFoods)

        Dim btnCreateAdmin As New Button With {
            .Text = "Admin Settings",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .BackColor = Color.DimGray,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point((Me.ClientSize.Width - 300) / 2, 300),
            .Size = New Size(300, 80) ' Larger button size
        }
        AddHandler btnCreateAdmin.Click, AddressOf btnCreateAdmin_Click
        Me.Controls.Add(btnCreateAdmin)

        ' Log Out Button
        Dim btnLogOut As New Button With {
            .Text = "Log Out",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .BackColor = Color.Red, ' Different color to distinguish it
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point((Me.ClientSize.Width - 300) / 2, 420), ' Adjusted position for better spacing
            .Size = New Size(300, 80) ' Larger button size
        }
        AddHandler btnLogOut.Click, AddressOf btnLogOut_Click
        Me.Controls.Add(btnLogOut)
    End Sub

    ' Button click events
    Private Sub btnManageWorkouts_Click(sender As Object, e As EventArgs)
        Me.Hide()
        Dim workoutPage As New WorkoutDatabaseViewer()
        workoutPage.Show()
    End Sub

    Private Sub btnManageFoods_Click(sender As Object, e As EventArgs)
        Me.Hide()
        Dim foodPage As New FoodDatabaseViewer()
        foodPage.Show()
    End Sub


    Private Sub btnCreateAdmin_Click(sender As Object, e As EventArgs)
        AdminSettingsForm.Show()
        Me.Hide()
    End Sub

    Private Sub btnLogOut_Click(sender As Object, e As EventArgs)
        Me.Hide()
        Form1.Show()
    End Sub
End Class
