Imports System.Windows.Forms

Public Class UserDashboard
    Private Sub UserDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Text = "User Dashboard"
        Me.Size = New Size(800, 600)
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(30, 30, 30)


        InitializeControls()

    End Sub

    Private Sub UserDashboard_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If Loginmessage Then
            MessageBox.Show($"Welcome, {CurrentUsername}!", "Nutrify", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Loginmessage = False
        End If
        If Logintrue Then
            MessageBox.Show($"You have been awarded 50 points for your first login today, {CurrentUsername}!", "Daily Reward", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Logintrue = False
        End If
        If MealLogtrue Then
            MessageBox.Show($"You have been awarded 50 points for your first meal logged today, {CurrentUsername}!", "Daily Reward", MessageBoxButtons.OK, MessageBoxIcon.Information)
            MealLogtrue = False
        End If
        If WorkoutLogtrue Then
            MessageBox.Show($"You have been awarded 50 points for your first workout logged today, {CurrentUsername}!", "Daily Reward", MessageBoxButtons.OK, MessageBoxIcon.Information)
            WorkoutLogtrue = False
        End If
    End Sub



    Private Sub InitializeControls()
        ' Title
        Dim lblTitle As New Label With {
            .Text = "Welcome to Nutrify!",
            .Font = New Font("Segoe UI", 24, FontStyle.Bold),
            .ForeColor = Color.LimeGreen,
            .Location = New Point((Me.ClientSize.Width - 300) / 2, 20),
            .AutoSize = True
        }
        Me.Controls.Add(lblTitle)

        ' Navigation Buttons
        Dim buttonWidth As Integer = 300
        Dim buttonHeight As Integer = 60
        Dim startX As Integer = (Me.ClientSize.Width - buttonWidth) / 2
        Dim startY As Integer = 100
        Dim spacing As Integer = 20

        ' Workouts Button
        Dim btnWorkouts As New Button With {
            .Text = "Workouts",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .BackColor = Color.LimeGreen,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(startX, startY),
            .Size = New Size(buttonWidth, buttonHeight)
        }
        AddHandler btnWorkouts.Click, AddressOf btnWorkouts_Click
        Me.Controls.Add(btnWorkouts)

        ' Nutrition Button
        Dim btnNutrition As New Button With {
            .Text = "Nutrition",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .BackColor = Color.LimeGreen,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(startX, startY + (buttonHeight + spacing) * 1),
            .Size = New Size(buttonWidth, buttonHeight)
        }
        AddHandler btnNutrition.Click, AddressOf btnNutrition_Click
        Me.Controls.Add(btnNutrition)

        ' Leaderboards Button
        Dim btnLeaderboards As New Button With {
            .Text = "Leaderboards",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .BackColor = Color.LimeGreen,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(startX, startY + (buttonHeight + spacing) * 2),
            .Size = New Size(buttonWidth, buttonHeight)
        }
        AddHandler btnLeaderboards.Click, AddressOf btnLeaderboards_Click
        Me.Controls.Add(btnLeaderboards)

        ' Settings Button 
        ' Settings Button 
        Dim btnSettings As New Button With {
            .Text = "Settings",
            .Font = New Font("Segoe UI", 14, FontStyle.Bold),
            .BackColor = Color.DimGray,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(startX, startY + (buttonHeight + spacing) * 3),
            .Size = New Size(buttonWidth, buttonHeight)
}

        AddHandler btnSettings.Click, AddressOf btnSettings_Click
        Me.Controls.Add(btnSettings)

        ' Logout Button
        Dim btnLogout As New Button With {
            .Text = "Logout",
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .BackColor = Color.DarkRed,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(startX, startY + (buttonHeight + spacing) * 4 + 10),
            .Size = New Size(buttonWidth, buttonHeight)
        }
        AddHandler btnLogout.Click, AddressOf btnLogout_Click
        Me.Controls.Add(btnLogout)
    End Sub


    Private Sub btnSettings_Click(sender As Object, e As EventArgs)
        Me.Hide()

        UserSettingsForm.Show()
    End Sub

    Private Sub btnWorkouts_Click(sender As Object, e As EventArgs)
        Dim workout As WorkoutForm = WorkoutForm.GetInstance()
        workout.Show()
        Me.Hide()
    End Sub

    Private Sub btnNutrition_Click(sender As Object, e As EventArgs)
        Me.Hide()
        ManageMeals.Show()
    End Sub

    Private Sub btnLeaderboards_Click(sender As Object, e As EventArgs)
        Me.Hide()
        Leaderboards.Show()
    End Sub

    Private Sub btnLogout_Click(sender As Object, e As EventArgs)
        Me.Hide()
        Form1.Show()
    End Sub
End Class
