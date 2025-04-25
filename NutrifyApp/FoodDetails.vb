Imports Npgsql
Imports System.Collections.Generic

Public Class FoodDetails
    Private foodId As Integer
    Private quantityServings As Integer = 1
    Private foodName As String = ""
    Private nutritionInfo As Dictionary(Of String, Object) = Nothing
    Private originalNutritionInfo As Dictionary(Of String, Object) = Nothing ' Store original values

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(ByVal foodId As Integer)
        InitializeComponent()
        Me.foodId = foodId
        Me.Text = "Food Details"
    End Sub

    Public Sub New(ByVal foodId As Integer, ByVal quantityServings As Integer)
        InitializeComponent()
        Me.foodId = foodId
        Me.quantityServings = quantityServings
        Me.Text = "Food Details"
    End Sub

    ' New constructor that accepts nutrition info directly
    Public Sub New(ByVal foodId As Integer, ByVal quantityServings As Integer, ByVal foodName As String, ByVal nutritionInfo As Dictionary(Of String, Object))
        InitializeComponent()
        Me.foodId = foodId
        Me.quantityServings = quantityServings
        Me.foodName = foodName
        Me.nutritionInfo = New Dictionary(Of String, Object)(nutritionInfo)
        Me.originalNutritionInfo = New Dictionary(Of String, Object)(nutritionInfo)
        Me.Text = "Food Details"
    End Sub

    Private Sub FoodDetails_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetupServingsControl()

        If nutritionInfo IsNot Nothing Then
            DisplayNutritionInfo()
        Else
            LoadFoodDetails()
        End If
    End Sub

    Private Sub SetupServingsControl()
        numServings.Minimum = 1
        numServings.Maximum = 99
        numServings.Value = quantityServings
        numServings.DecimalPlaces = 0
        numServings.Increment = 1
        numServings.ReadOnly = False

        AddHandler numServings.ValueChanged, AddressOf numServings_ValueChanged
    End Sub

    Private Sub numServings_ValueChanged(sender As Object, e As EventArgs)
        quantityServings = Convert.ToInt32(numServings.Value)

        UpdateNutritionDisplay()

        Me.Text = $"Food Details - {txtFoodName.Text} ({quantityServings} serving{If(quantityServings > 1, "s", "")})"
    End Sub

    ' New method to update nutrition display based on current serving count
    Private Sub UpdateNutritionDisplay()
        lblServings.Text = "Servings: "

        txtQuantity.Text = FormatValue(ScaleValue(GetNutritionValue("quantity_grams")), "g")
        txtCalories.Text = FormatValue(ScaleValue(GetNutritionValue("calories")), "kcal")
        txtProtein.Text = FormatValue(ScaleValue(GetNutritionValue("protein")), "g")
        txtCarbs.Text = FormatValue(ScaleValue(GetNutritionValue("carbs")), "g")
        txtFat.Text = FormatValue(ScaleValue(GetNutritionValue("fat")), "g")
        txtFiber.Text = FormatValue(ScaleValue(GetNutritionValue("fiber")), "g")
        txtSugar.Text = FormatValue(ScaleValue(GetNutritionValue("sugar")), "g")
        txtSodium.Text = FormatValue(ScaleValue(GetNutritionValue("sodium")), "mg")
        txtSaturatedFat.Text = FormatValue(ScaleValue(GetNutritionValue("saturated_fat")), "g")
        txtCholesterol.Text = FormatValue(ScaleValue(GetNutritionValue("cholesterol")), "mg")
        txtPotassium.Text = FormatValue(ScaleValue(GetNutritionValue("potassium")), "mg")
        txtCalcium.Text = FormatValue(ScaleValue(GetNutritionValue("calcium")), "mg")
    End Sub

    ' Display nutrition information from the dictionary that was passed in
    Private Sub DisplayNutritionInfo()
        txtFoodName.Text = foodName

        UpdateNutritionDisplay()
    End Sub

    ' Helper function to safely get nutrition values from the dictionary
    Private Function GetNutritionValue(key As String) As Object
        If nutritionInfo IsNot Nothing AndAlso nutritionInfo.ContainsKey(key) AndAlso nutritionInfo(key) IsNot Nothing Then
            Return nutritionInfo(key)
        ElseIf originalNutritionInfo IsNot Nothing AndAlso originalNutritionInfo.ContainsKey(key) AndAlso originalNutritionInfo(key) IsNot Nothing Then
            Return originalNutritionInfo(key)
        Else
            Return Nothing
        End If
    End Function

    ' Original database loading method (fallback if nutrition info not provided)
    Private Sub LoadFoodDetails()
        Dim connString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;"

        If foodId <= 0 Then
            MessageBox.Show("Invalid food ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
            Return
        End If

        Try
            Using conn As New NpgsqlConnection(connString)
                conn.Open()

                Dim query As String = "SELECT f.name, f.quantity_grams, f.calories, f.protein, f.carbs, f.fat, " &
                                 "f.fiber, f.sugar, f.sodium, f.saturated_fat, f.cholesterol, f.potassium, f.calcium " &
                                 "FROM Foods f WHERE f.food_id = @foodId"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@foodId", foodId)

                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            txtFoodName.Text = reader("name").ToString()

                            nutritionInfo = New Dictionary(Of String, Object)()
                            originalNutritionInfo = New Dictionary(Of String, Object)()

                            nutritionInfo.Add("quantity_grams", reader("quantity_grams"))
                            nutritionInfo.Add("calories", reader("calories"))
                            nutritionInfo.Add("protein", reader("protein"))
                            nutritionInfo.Add("carbs", reader("carbs"))
                            nutritionInfo.Add("fat", reader("fat"))
                            nutritionInfo.Add("fiber", reader("fiber"))
                            nutritionInfo.Add("sugar", reader("sugar"))
                            nutritionInfo.Add("sodium", reader("sodium"))
                            nutritionInfo.Add("saturated_fat", reader("saturated_fat"))
                            nutritionInfo.Add("cholesterol", reader("cholesterol"))
                            nutritionInfo.Add("potassium", reader("potassium"))
                            nutritionInfo.Add("calcium", reader("calcium"))

                            For Each kvp In nutritionInfo
                                originalNutritionInfo.Add(kvp.Key, kvp.Value)
                            Next

                            UpdateNutritionDisplay()

                            Me.Text = $"Food Details - {txtFoodName.Text} ({quantityServings} serving{If(quantityServings > 1, "s", "")})"
                        Else
                            MessageBox.Show($"Food not found with ID: {foodId}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Me.Close()
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show("Error loading food details: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
        End Try
    End Sub

    Private Function ScaleValue(value As Object) As Object
        If value Is Nothing OrElse value Is DBNull.Value Then Return "N/A"
        Try
            Return Convert.ToDecimal(value) * quantityServings
        Catch
            Return value
        End Try
    End Function

    Private Function FormatValue(value As Object, unit As String) As String
        If value Is Nothing OrElse value Is DBNull.Value OrElse value.ToString() = "N/A" Then Return "N/A"
        Return $"{value} {unit}"
    End Function

    Private Sub CloseButton_Click(sender As Object, e As EventArgs) Handles CloseButton.Click
        Me.Close()
    End Sub

End Class