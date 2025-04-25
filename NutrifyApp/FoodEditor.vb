Imports System.Windows.Forms
Imports System.Drawing
Imports System.Data
Imports Npgsql
Imports System.Threading.Tasks
Imports System.ComponentModel
Imports System.Net.NetworkInformation

Public Class FoodEditor
    ' Colors
    Private PrimaryColor As Color = Color.LimeGreen
    Private ButtonColor As Color = Color.LimeGreen
    Private BackButtonColor As Color = Color.DarkRed
    Private ErrorColor As Color = Color.Red

    ' UI Controls
    Private WithEvents lblTitle As Label
    Private WithEvents lblName As Label
    Private WithEvents txtName As TextBox
    Private WithEvents lblBarcode As Label
    Private WithEvents txtBarcode As TextBox
    Private WithEvents lblQuantity As Label
    Private WithEvents txtQuantity As TextBox
    Private WithEvents lblCalories As Label
    Private WithEvents txtCalories As TextBox
    Private WithEvents lblProtein As Label
    Private WithEvents txtProtein As TextBox
    Private WithEvents lblCarbs As Label
    Private WithEvents txtCarbs As TextBox
    Private WithEvents lblFat As Label
    Private WithEvents txtFat As TextBox
    Private WithEvents lblFiber As Label
    Private WithEvents txtFiber As TextBox
    Private WithEvents lblSugar As Label
    Private WithEvents txtSugar As TextBox
    Private WithEvents lblSodium As Label
    Private WithEvents txtSodium As TextBox
    Private WithEvents lblSaturatedFat As Label
    Private WithEvents txtSaturatedFat As TextBox
    Private WithEvents lblCholesterol As Label
    Private WithEvents txtCholesterol As TextBox
    Private WithEvents lblPotassium As Label
    Private WithEvents txtPotassium As TextBox
    Private WithEvents lblCalcium As Label
    Private WithEvents txtCalcium As TextBox
    Private WithEvents cmdSave As Button
    Private WithEvents cmdCancel As Button
    Private WithEvents lblStatus As Label

    ' Data
    Private FoodId As Integer
    Private IsNewFood As Boolean
    Private CurrentFood As DataRow = Nothing
    Private ValidationErrors As New List(Of String)
    Private IsLoading As Boolean = True
    Private ConnectionString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=2;Maximum Pool Size=10;Timeout=15;Command Timeout=15;"

    Public Sub New(foodId As Integer)
        ' This call is required by the designer.
        InitializeComponentFE()

        ' Set the food ID and determine if this is a new food
        Me.FoodId = foodId
        Me.IsNewFood = (foodId = 0)

        ' Set the form title based on whether we're adding or editing
        Me.Text = If(IsNewFood, "Add New Food", "Edit Food")

        ' Load food data or prepare for a new food
        If IsNewFood Then
            Me.lblTitle.Text = "Add New Food"
            IsLoading = False
        Else
            Me.lblTitle.Text = "Edit Food"
            LoadFoodData()
        End If
    End Sub

    Private Sub InitializeComponentFE()
        Me.Size = New Size(850, 650)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.BackColor = Color.FromArgb(30, 30, 30)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        ' === Title Label ===
        lblTitle = New Label With {
            .Text = "Food Details",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = PrimaryColor,
            .AutoSize = True,
            .Location = New Point(30, 20)
        }
        Me.Controls.Add(lblTitle)

        ' === Create Label-TextBox Pairs ===
        Dim yPosition As Integer = 80
        Dim spacing As Integer = 55

        ' === Name ===
        lblName = CreateLabel("Food Name:", 30, yPosition)
        txtName = CreateTextBox(180, yPosition, 250)
        Me.Controls.Add(lblName)
        Me.Controls.Add(txtName)
        yPosition += spacing

        ' === Barcode ===
        lblBarcode = CreateLabel("Barcode:", 30, yPosition)
        txtBarcode = CreateTextBox(180, yPosition, 250)
        Me.Controls.Add(lblBarcode)
        Me.Controls.Add(txtBarcode)
        yPosition += spacing

        ' === Quantity ===
        lblQuantity = CreateLabel("Serving Size (g):", 30, yPosition)
        txtQuantity = CreateTextBox(180, yPosition, 100)
        Me.Controls.Add(lblQuantity)
        Me.Controls.Add(txtQuantity)
        yPosition += spacing

        ' === Calories ===
        lblCalories = CreateLabel("Calories:", 30, yPosition)
        txtCalories = CreateTextBox(180, yPosition, 100)
        Me.Controls.Add(lblCalories)
        Me.Controls.Add(txtCalories)
        yPosition += spacing

        ' Create right column
        yPosition = 80

        ' === Protein ===
        lblProtein = CreateLabel("Protein (g):", 450, yPosition)
        txtProtein = CreateTextBox(600, yPosition, 100)
        Me.Controls.Add(lblProtein)
        Me.Controls.Add(txtProtein)
        yPosition += spacing

        ' === Carbs ===
        lblCarbs = CreateLabel("Carbs (g):", 450, yPosition)
        txtCarbs = CreateTextBox(600, yPosition, 100)
        Me.Controls.Add(lblCarbs)
        Me.Controls.Add(txtCarbs)
        yPosition += spacing

        ' === Fat ===
        lblFat = CreateLabel("Fat (g):", 450, yPosition)
        txtFat = CreateTextBox(600, yPosition, 100)
        Me.Controls.Add(lblFat)
        Me.Controls.Add(txtFat)
        yPosition += spacing

        ' === Fiber ===
        lblFiber = CreateLabel("Fiber (g):", 450, yPosition)
        txtFiber = CreateTextBox(600, yPosition, 100)
        Me.Controls.Add(lblFiber)
        Me.Controls.Add(txtFiber)
        yPosition += spacing

        ' Start a new column below for other nutrients
        yPosition = 300

        ' === Sugar ===
        lblSugar = CreateLabel("Sugar (g):", 30, yPosition)
        txtSugar = CreateTextBox(180, yPosition, 100)
        Me.Controls.Add(lblSugar)
        Me.Controls.Add(txtSugar)
        yPosition += spacing

        ' === Sodium ===
        lblSodium = CreateLabel("Sodium (mg):", 30, yPosition)
        txtSodium = CreateTextBox(180, yPosition, 100)
        Me.Controls.Add(lblSodium)
        Me.Controls.Add(txtSodium)
        yPosition += spacing

        ' === Saturated Fat ===
        lblSaturatedFat = CreateLabel("Saturated Fat (g):", 30, yPosition)
        txtSaturatedFat = CreateTextBox(180, yPosition, 100)
        Me.Controls.Add(lblSaturatedFat)
        Me.Controls.Add(txtSaturatedFat)
        yPosition += spacing

        ' Create matching column on right
        yPosition = 300

        ' === Cholesterol ===
        lblCholesterol = CreateLabel("Cholesterol (mg):", 450, yPosition)
        txtCholesterol = CreateTextBox(600, yPosition, 100)
        Me.Controls.Add(lblCholesterol)
        Me.Controls.Add(txtCholesterol)
        yPosition += spacing

        ' === Potassium ===
        lblPotassium = CreateLabel("Potassium (mg):", 450, yPosition)
        txtPotassium = CreateTextBox(600, yPosition, 100)
        Me.Controls.Add(lblPotassium)
        Me.Controls.Add(txtPotassium)
        yPosition += spacing

        ' === Calcium ===
        lblCalcium = CreateLabel("Calcium (mg):", 450, yPosition)
        txtCalcium = CreateTextBox(600, yPosition, 100)
        Me.Controls.Add(lblCalcium)
        Me.Controls.Add(txtCalcium)
        yPosition += spacing

        ' === Status Label ===
        lblStatus = New Label With {
            .Font = New Font("Segoe UI", 11),
            .ForeColor = Color.White,
            .Location = New Point(30, 500),
            .Size = New Size(780, 40),
            .Text = "Enter food details and click Save."
        }
        Me.Controls.Add(lblStatus)

        ' === Save Button ===
        cmdSave = New Button With {
            .Text = "Save",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(600, 570),
            .Size = New Size(100, 36)
        }
        Me.Controls.Add(cmdSave)

        ' === Cancel Button ===
        cmdCancel = New Button With {
            .Text = "Cancel",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = BackButtonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(710, 570),
            .Size = New Size(100, 36)
        }
        Me.Controls.Add(cmdCancel)

        ' Add event handlers
        AddHandler cmdSave.Click, AddressOf cmdSave_Click
        AddHandler cmdCancel.Click, AddressOf cmdCancel_Click
    End Sub

    Private Function CreateLabel(text As String, x As Integer, y As Integer) As Label
        Return New Label With {
            .Text = text,
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(x, y),
            .AutoSize = True
        }
    End Function

    Private Function CreateTextBox(x As Integer, y As Integer, width As Integer) As TextBox
        Return New TextBox With {
            .Font = New Font("Segoe UI", 11),
            .Location = New Point(x, y),
            .Width = width,
            .BackColor = Color.FromArgb(45, 45, 45),
            .ForeColor = Color.White
        }
    End Function

    Private Async Sub LoadFoodData()
        IsLoading = True
        Me.Cursor = Cursors.WaitCursor
        lblStatus.Text = "Loading food data..."

        Try
            ' Check network connectivity first
            If Not CheckNetworkConnectivity() Then
                lblStatus.Text = "Network connection issue. Please check your internet connection."
                lblStatus.ForeColor = ErrorColor
                IsLoading = False
                Me.Cursor = Cursors.Default
                Return
            End If

            ' Try to load food data with timeout
            Dim loadTask = Task.Run(Function() GetFoodFromDatabase(FoodId))

            ' Add timeout to the task
            If Await Task.WhenAny(loadTask, Task.Delay(8000)) Is loadTask Then
                ' Task completed within timeout
                Dim foodData As DataRow = loadTask.Result

                If foodData IsNot Nothing Then
                    CurrentFood = foodData

                    ' Populate the form fields
                    txtName.Text = Convert.ToString(foodData("name"))

                    If Not IsDBNull(foodData("barcode")) Then
                        txtBarcode.Text = Convert.ToString(foodData("barcode"))
                    End If

                    If Not IsDBNull(foodData("quantity_grams")) Then
                        txtQuantity.Text = Convert.ToString(foodData("quantity_grams"))
                    End If

                    If Not IsDBNull(foodData("calories")) Then
                        txtCalories.Text = Convert.ToString(foodData("calories"))
                    End If

                    If Not IsDBNull(foodData("protein")) Then
                        txtProtein.Text = Convert.ToString(foodData("protein"))
                    End If

                    If Not IsDBNull(foodData("carbs")) Then
                        txtCarbs.Text = Convert.ToString(foodData("carbs"))
                    End If

                    If Not IsDBNull(foodData("fat")) Then
                        txtFat.Text = Convert.ToString(foodData("fat"))
                    End If

                    If Not IsDBNull(foodData("fiber")) Then
                        txtFiber.Text = Convert.ToString(foodData("fiber"))
                    End If

                    If Not IsDBNull(foodData("sugar")) Then
                        txtSugar.Text = Convert.ToString(foodData("sugar"))
                    End If

                    If Not IsDBNull(foodData("sodium")) Then
                        txtSodium.Text = Convert.ToString(foodData("sodium"))
                    End If

                    If Not IsDBNull(foodData("saturated_fat")) Then
                        txtSaturatedFat.Text = Convert.ToString(foodData("saturated_fat"))
                    End If

                    If Not IsDBNull(foodData("cholesterol")) Then
                        txtCholesterol.Text = Convert.ToString(foodData("cholesterol"))
                    End If

                    If Not IsDBNull(foodData("potassium")) Then
                        txtPotassium.Text = Convert.ToString(foodData("potassium"))
                    End If

                    If Not IsDBNull(foodData("calcium")) Then
                        txtCalcium.Text = Convert.ToString(foodData("calcium"))
                    End If

                    lblStatus.Text = $"Editing {txtName.Text}"
                Else
                    lblStatus.Text = "Could not find the selected food in the database."
                    lblStatus.ForeColor = ErrorColor
                End If
            Else
                ' Task timed out
                lblStatus.Text = "Database connection timed out. Please try again later."
                lblStatus.ForeColor = ErrorColor
            End If

        Catch ex As Exception
            HandleDatabaseException(ex, "loading food data")
        Finally
            Me.Cursor = Cursors.Default
            IsLoading = False
        End Try
    End Sub

    Private Function CheckNetworkConnectivity() As Boolean
        Try
            ' Try to ping Google's DNS to check internet connectivity
            Using ping As New Ping()
                Dim reply As PingReply = ping.Send("8.8.8.8", 3000)
                Return (reply.Status = IPStatus.Success)
            End Using
        Catch ex As Exception
            ' If any exception occurs during ping, assume network is down
            Return False
        End Try
    End Function

    Private Function GetFoodFromDatabase(foodId As Integer) As DataRow
        Try
            Using conn As New NpgsqlConnection(ConnectionString)
                ' Set short timeouts to avoid long waits
                conn.Open()

                Dim query As String = "SELECT * FROM Foods WHERE food_id = @food_id;"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 5  ' Set command timeout to 5 seconds
                    cmd.Parameters.AddWithValue("@food_id", foodId)

                    Using reader = cmd.ExecuteReader()
                        Dim foodTable As New DataTable()
                        foodTable.Load(reader)
                        If foodTable.Rows.Count > 0 Then
                            Return foodTable.Rows(0)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As NpgsqlException
            ' Log the specific database error but don't throw it
            System.Diagnostics.Debug.WriteLine("Database error: " & ex.Message)
            If ex.InnerException IsNot Nothing Then
                System.Diagnostics.Debug.WriteLine("Inner exception: " & ex.InnerException.Message)
            End If
            ' Let the caller handle the null return
        Catch ex As Exception
            ' Log general errors
            System.Diagnostics.Debug.WriteLine("General error: " & ex.Message)
        End Try

        Return Nothing
    End Function

    Private Sub cmdCancel_Click(sender As Object, e As EventArgs)
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Async Sub cmdSave_Click(sender As Object, e As EventArgs)
        If ValidateInput() = False Then
            ' Show the validation errors
            lblStatus.Text = String.Join("; ", ValidationErrors)
            lblStatus.ForeColor = ErrorColor
            Return
        End If

        Me.Cursor = Cursors.WaitCursor
        lblStatus.Text = "Saving food data..."
        lblStatus.ForeColor = Color.White

        Try
            ' Check network connectivity first
            If Not CheckNetworkConnectivity() Then
                lblStatus.Text = "Network connection issue. Please check your internet connection."
                lblStatus.ForeColor = ErrorColor
                Me.Cursor = Cursors.Default
                Return
            End If

            ' Get the data from the form
            Dim name As String = txtName.Text.Trim()
            Dim barcode As String = txtBarcode.Text.Trim()

            ' Parse numeric fields safely
            Dim quantity As Integer? = ParseNullableInteger(txtQuantity.Text)
            Dim calories As Integer? = ParseNullableInteger(txtCalories.Text)
            Dim protein As Decimal? = ParseNullableDecimal(txtProtein.Text)
            Dim carbs As Decimal? = ParseNullableDecimal(txtCarbs.Text)
            Dim fat As Decimal? = ParseNullableDecimal(txtFat.Text)
            Dim fiber As Decimal? = ParseNullableDecimal(txtFiber.Text)
            Dim sugar As Decimal? = ParseNullableDecimal(txtSugar.Text)
            Dim sodium As Decimal? = ParseNullableDecimal(txtSodium.Text)
            Dim saturatedFat As Decimal? = ParseNullableDecimal(txtSaturatedFat.Text)
            Dim cholesterol As Decimal? = ParseNullableDecimal(txtCholesterol.Text)
            Dim potassium As Decimal? = ParseNullableDecimal(txtPotassium.Text)
            Dim calcium As Decimal? = ParseNullableDecimal(txtCalcium.Text)

            ' Try to save with timeout
            Dim saveTask As Task(Of Boolean)
            If IsNewFood Then
                saveTask = Task.Run(Function() InsertFoodIntoDatabase(name, barcode, quantity, calories, protein, carbs, fat, fiber, sugar, sodium, saturatedFat, cholesterol, potassium, calcium))
            Else
                saveTask = Task.Run(Function() UpdateFoodInDatabase(FoodId, name, barcode, quantity, calories, protein, carbs, fat, fiber, sugar, sodium, saturatedFat, cholesterol, potassium, calcium))
            End If

            ' Add timeout to the save task
            If Await Task.WhenAny(saveTask, Task.Delay(8000)) Is saveTask Then
                ' Task completed within timeout
                Dim success As Boolean = saveTask.Result

                If success Then
                    Me.DialogResult = DialogResult.OK
                    Me.Close()
                Else
                    lblStatus.Text = "Error saving data to the database. Please try again."
                    lblStatus.ForeColor = ErrorColor
                End If
            Else
                ' Task timed out
                lblStatus.Text = "Database operation timed out. Please try again later."
                lblStatus.ForeColor = ErrorColor
            End If

        Catch ex As Exception
            HandleDatabaseException(ex, "saving food data")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub HandleDatabaseException(ex As Exception, operation As String)
        Dim errorMessage As String = $"Error {operation}: "

        If TypeOf ex Is NpgsqlException Then
            Dim npgEx As NpgsqlException = DirectCast(ex, NpgsqlException)
            errorMessage &= "Database connection problem. "

            If npgEx.InnerException IsNot Nothing Then
                If TypeOf npgEx.InnerException Is System.Net.Sockets.SocketException Then
                    errorMessage &= "Network connection issue. Please check your internet connection."
                ElseIf TypeOf npgEx.InnerException Is System.IO.IOException Then
                    errorMessage &= "Connection timed out. Please try again later."
                Else
                    errorMessage &= "Please try again later."
                End If
            Else
                errorMessage &= "Please try again later."
            End If
        Else
            errorMessage &= ex.Message
        End If

        System.Diagnostics.Debug.WriteLine(errorMessage)
        System.Diagnostics.Debug.WriteLine(ex.ToString())

        lblStatus.Text = errorMessage
        lblStatus.ForeColor = ErrorColor
    End Sub

    Private Function ValidateInput() As Boolean
        ValidationErrors.Clear()

        ' Required field validation
        If String.IsNullOrWhiteSpace(txtName.Text) Then
            ValidationErrors.Add("Food name is required")
        End If

        ' Numeric field validation
        CheckNumericField(txtQuantity.Text, "Serving size")
        CheckNumericField(txtCalories.Text, "Calories")
        CheckDecimalField(txtProtein.Text, "Protein")
        CheckDecimalField(txtCarbs.Text, "Carbs")
        CheckDecimalField(txtFat.Text, "Fat")
        CheckDecimalField(txtFiber.Text, "Fiber")
        CheckDecimalField(txtSugar.Text, "Sugar")
        CheckDecimalField(txtSodium.Text, "Sodium")
        CheckDecimalField(txtSaturatedFat.Text, "Saturated fat")
        CheckDecimalField(txtCholesterol.Text, "Cholesterol")
        CheckDecimalField(txtPotassium.Text, "Potassium")
        CheckDecimalField(txtCalcium.Text, "Calcium")

        Return ValidationErrors.Count = 0
    End Function

    Private Sub CheckNumericField(value As String, fieldName As String)
        If Not String.IsNullOrWhiteSpace(value) Then
            Dim numValue As Integer
            If Not Integer.TryParse(value, numValue) Then
                ValidationErrors.Add($"{fieldName} must be a whole number")
            ElseIf numValue < 0 Then
                ValidationErrors.Add($"{fieldName} cannot be negative")
            End If
        End If
    End Sub

    Private Sub CheckDecimalField(value As String, fieldName As String)
        If Not String.IsNullOrWhiteSpace(value) Then
            Dim numValue As Decimal
            If Not Decimal.TryParse(value, numValue) Then
                ValidationErrors.Add($"{fieldName} must be a number")
            ElseIf numValue < 0 Then
                ValidationErrors.Add($"{fieldName} cannot be negative")
            End If
        End If
    End Sub

    Private Function ParseNullableInteger(text As String) As Integer?
        If String.IsNullOrWhiteSpace(text) Then
            Return Nothing
        End If

        Dim result As Integer
        If Integer.TryParse(text, result) Then
            Return result
        End If

        Return Nothing
    End Function

    Private Function ParseNullableDecimal(text As String) As Decimal?
        If String.IsNullOrWhiteSpace(text) Then
            Return Nothing
        End If

        Dim result As Decimal
        If Decimal.TryParse(text, result) Then
            Return result
        End If

        Return Nothing
    End Function

    Private Function InsertFoodIntoDatabase(
        name As String, barcode As String,
        quantity As Integer?, calories As Integer?,
        protein As Decimal?, carbs As Decimal?,
        fat As Decimal?, fiber As Decimal?,
        sugar As Decimal?, sodium As Decimal?,
        saturatedFat As Decimal?, cholesterol As Decimal?,
        potassium As Decimal?, calcium As Decimal?) As Boolean

        Try
            Using conn As New NpgsqlConnection(ConnectionString)
                conn.Open()

                Dim query As String = "
                    INSERT INTO Foods (
                        name, barcode, quantity_grams, calories, protein, carbs, fat, 
                        fiber, sugar, sodium, saturated_fat, cholesterol, potassium, calcium
                    ) VALUES (
                        @name, @barcode, @quantity, @calories, @protein, @carbs, @fat,
                        @fiber, @sugar, @sodium, @saturated_fat, @cholesterol, @potassium, @calcium
                    );"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 5  ' Short timeout
                    cmd.Parameters.AddWithValue("@name", name)

                    If String.IsNullOrEmpty(barcode) Then
                        cmd.Parameters.AddWithValue("@barcode", DBNull.Value)
                    Else
                        cmd.Parameters.AddWithValue("@barcode", barcode)
                    End If

                    AddNullableParameter(cmd, "@quantity", quantity)
                    AddNullableParameter(cmd, "@calories", calories)
                    AddNullableParameter(cmd, "@protein", protein)
                    AddNullableParameter(cmd, "@carbs", carbs)
                    AddNullableParameter(cmd, "@fat", fat)
                    AddNullableParameter(cmd, "@fiber", fiber)
                    AddNullableParameter(cmd, "@sugar", sugar)
                    AddNullableParameter(cmd, "@sodium", sodium)
                    AddNullableParameter(cmd, "@saturated_fat", saturatedFat)
                    AddNullableParameter(cmd, "@cholesterol", cholesterol)
                    AddNullableParameter(cmd, "@potassium", potassium)
                    AddNullableParameter(cmd, "@calcium", calcium)

                    cmd.ExecuteNonQuery()
                    Return True
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error inserting food: " & ex.Message)
            If TypeOf ex Is NpgsqlException AndAlso ex.InnerException IsNot Nothing Then
                System.Diagnostics.Debug.WriteLine("Inner exception: " & ex.InnerException.Message)
            End If
            Return False
        End Try
    End Function

    Private Function UpdateFoodInDatabase(
        foodId As Integer, name As String, barcode As String,
        quantity As Integer?, calories As Integer?,
        protein As Decimal?, carbs As Decimal?,
        fat As Decimal?, fiber As Decimal?,
        sugar As Decimal?, sodium As Decimal?,
        saturatedFat As Decimal?, cholesterol As Decimal?,
        potassium As Decimal?, calcium As Decimal?) As Boolean

        Try
            Using conn As New NpgsqlConnection(ConnectionString)
                conn.Open()

                Dim query As String = "
                    UPDATE Foods SET
                        name = @name,
                        barcode = @barcode,
                        quantity_grams = @quantity,
                        calories = @calories,
                        protein = @protein,
                        carbs = @carbs,
                        fat = @fat,
                        fiber = @fiber,
                        sugar = @sugar,
                        sodium = @sodium,
                        saturated_fat = @saturated_fat,
                        cholesterol = @cholesterol,
                        potassium = @potassium,
                        calcium = @calcium
                    WHERE food_id = @food_id;"

                Using cmd As New NpgsqlCommand(query, conn)
                    cmd.CommandTimeout = 5  ' Short timeout
                    cmd.Parameters.AddWithValue("@food_id", foodId)
                    cmd.Parameters.AddWithValue("@name", name)

                    If String.IsNullOrEmpty(barcode) Then
                        cmd.Parameters.AddWithValue("@barcode", DBNull.Value)
                    Else
                        cmd.Parameters.AddWithValue("@barcode", barcode)
                    End If

                    AddNullableParameter(cmd, "@quantity", quantity)
                    AddNullableParameter(cmd, "@calories", calories)
                    AddNullableParameter(cmd, "@protein", protein)
                    AddNullableParameter(cmd, "@carbs", carbs)
                    AddNullableParameter(cmd, "@fat", fat)
                    AddNullableParameter(cmd, "@fiber", fiber)
                    AddNullableParameter(cmd, "@sugar", sugar)
                    AddNullableParameter(cmd, "@sodium", sodium)
                    AddNullableParameter(cmd, "@saturated_fat", saturatedFat)
                    AddNullableParameter(cmd, "@cholesterol", cholesterol)
                    AddNullableParameter(cmd, "@potassium", potassium)
                    AddNullableParameter(cmd, "@calcium", calcium)

                    cmd.ExecuteNonQuery()
                    Return True
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error updating food: " & ex.Message)
            If TypeOf ex Is NpgsqlException AndAlso ex.InnerException IsNot Nothing Then
                System.Diagnostics.Debug.WriteLine("Inner exception: " & ex.InnerException.Message)
            End If
            Return False
        End Try
    End Function

    Private Sub AddNullableParameter(cmd As NpgsqlCommand, paramName As String, value As Object)
        If value Is Nothing Then
            cmd.Parameters.AddWithValue(paramName, DBNull.Value)
        Else
            cmd.Parameters.AddWithValue(paramName, value)
        End If
    End Sub
End Class