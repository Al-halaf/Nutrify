Imports Microsoft.VisualBasic.ApplicationServices
Imports Npgsql
Imports System.Windows.Forms
Imports System.Threading.Tasks
Imports System.Diagnostics
Imports System.Data
Imports System.Collections.Generic
Imports System.Threading
Imports System.IO
Imports System.Text

Public Class ManageMeals
    Private UserId As Integer = CurrUserID

    Private WithEvents SearchModeLabel As New Label
    Private IsDateFilterActive As Boolean = False
    Private SelectedFilterDate As DateTime = DateTime.Today
    Private FilterMode As Integer = 0  ' 0 = Today, 1 = Custom Date, 2 = No Filter

    Private AllData As DataTable = Nothing
    Private mealCount As Integer = 0

    Private DatabaseOperationInProgress As Boolean = False

    Private RetryQueue As New Queue(Of Action)

    Private CancellationSource As New CancellationTokenSource()

    Private Sub ManageMeals_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.ReadOnly = True
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.AllowUserToDeleteRows = False
        DataGridView1.MultiSelect = False

        SetupSearchUI()
        InitializeFilterControls()

        LoadAllDataFromDatabase()

        StartRetryProcessor()
    End Sub

    Private Sub ManageMeals_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        CancellationSource.Cancel()
    End Sub

    Private Async Sub StartRetryProcessor()
        Await Task.Run(AddressOf ProcessRetryQueue)
    End Sub

    Private Async Function ProcessRetryQueue() As Task
        While Not CancellationSource.Token.IsCancellationRequested
            If RetryQueue.Count > 0 AndAlso Not DatabaseOperationInProgress Then
                DatabaseOperationInProgress = True
                Try
                    Dim operation As Action = RetryQueue.Dequeue()
                    operation()
                Catch ex As Exception
                    Debug.WriteLine("Retry operation failed: " & ex.Message)
                Finally
                    DatabaseOperationInProgress = False
                End Try
            End If
            Await Task.Delay(100, CancellationSource.Token)
        End While
    End Function

    Private Sub InitializeFilterControls()
        DateFilterComboBox.Items.Clear()
        DateFilterComboBox.Items.Add("Today's Meals")
        DateFilterComboBox.Items.Add("Custom Date")
        DateFilterComboBox.Items.Add("No Filter")
        DateFilterComboBox.SelectedIndex = 0  ' Default to today's meals

        DateFilterPicker.Format = DateTimePickerFormat.Short
        DateFilterPicker.Value = DateTime.Today
        DateFilterPicker.Enabled = False  ' Disabled initially, since "Today's Meals" is selected

        SelectedFilterDate = DateTime.Today
        IsDateFilterActive = True
        FilterMode = 0  ' Today mode
    End Sub

    Private Sub SetupSearchUI()
        SearchModeLabel.AutoSize = True
        SearchModeLabel.ForeColor = System.Drawing.Color.White
        SearchModeLabel.Location = New System.Drawing.Point(SearchTextBox.Left, SearchTextBox.Bottom + 5)
        SearchModeLabel.Visible = False
        Me.Controls.Add(SearchModeLabel)

        ClearSearchButton.Visible = False
    End Sub

    ' Load ALL data from the database in a single query
    Private Async Sub LoadAllDataFromDatabase()
        Try
            AllData = New DataTable()
            AllData.Columns.Add("food_id", GetType(Integer))
            AllData.Columns.Add("Name", GetType(String))
            AllData.Columns.Add("Date", GetType(DateTime))
            AllData.Columns.Add("Quantity", GetType(Integer))
            AllData.Columns.Add("has_history", GetType(Boolean))
            AllData.Columns.Add("barcode", GetType(String))

            AllData.Columns.Add("quantity_grams", GetType(Decimal))
            AllData.Columns.Add("calories", GetType(Decimal))
            AllData.Columns.Add("protein", GetType(Decimal))
            AllData.Columns.Add("carbs", GetType(Decimal))
            AllData.Columns.Add("fat", GetType(Decimal))
            AllData.Columns.Add("fiber", GetType(Decimal))
            AllData.Columns.Add("sugar", GetType(Decimal))
            AllData.Columns.Add("sodium", GetType(Decimal))
            AllData.Columns.Add("saturated_fat", GetType(Decimal))
            AllData.Columns.Add("cholesterol", GetType(Decimal))
            AllData.Columns.Add("potassium", GetType(Decimal))
            AllData.Columns.Add("calcium", GetType(Decimal))

            AllData.Columns("Date").AllowDBNull = True
            AllData.Columns("Quantity").AllowDBNull = True

            AllData.Columns("barcode").AllowDBNull = True

            AllData.Columns("quantity_grams").AllowDBNull = True
            AllData.Columns("calories").AllowDBNull = True
            AllData.Columns("protein").AllowDBNull = True
            AllData.Columns("carbs").AllowDBNull = True
            AllData.Columns("fat").AllowDBNull = True
            AllData.Columns("fiber").AllowDBNull = True
            AllData.Columns("sugar").AllowDBNull = True
            AllData.Columns("sodium").AllowDBNull = True
            AllData.Columns("saturated_fat").AllowDBNull = True
            AllData.Columns("cholesterol").AllowDBNull = True
            AllData.Columns("potassium").AllowDBNull = True
            AllData.Columns("calcium").AllowDBNull = True

            ' Run the database query in a background thread
            Await Task.Run(Function() LoadDataFromDatabaseAsync())

            ' Now that we have all data, apply the initial filters
            ApplyFiltersLocally()

        Catch ex As Exception
            Debug.WriteLine("Error loading data: " & ex.Message)
            ' Show a generic message to the user instead of the full error
            MessageBox.Show("Could not load all meal data. Please try again later.", "Data Loading Issue", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
    Private Async Function LoadDataFromDatabaseAsync() As Task
        Dim connString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;"
        Dim maxRetries As Integer = 3
        Dim retryCount As Integer = 0

        While retryCount < maxRetries
            Try
                Using conn As New NpgsqlConnection(connString)
                    Await conn.OpenAsync()

                    Dim query As String = "
                    -- All foods the user has consumed (with dates, quantities, and nutrition info)
                    SELECT 
                        f.food_id, 
                        f.name, 
                        uf.date_consumed, 
                        uf.quantity_servings,
                        TRUE as has_history,
                        f.barcode,
                        f.quantity_grams,
                        f.calories,
                        f.protein,
                        f.carbs,
                        f.fat,
                        f.fiber,
                        f.sugar,
                        f.sodium,
                        f.saturated_fat,
                        f.cholesterol,
                        f.potassium,
                        f.calcium
                    FROM 
                        Foods f 
                    JOIN 
                        User_Foods uf ON f.food_id = uf.food_id 
                    WHERE 
                        uf.user_id = @UserId
                    
                    UNION ALL
                    
                    -- All foods the user has NOT consumed (without dates and quantities, but with nutrition info)
                    SELECT 
                        f.food_id, 
                        f.name, 
                        NULL as date_consumed, 
                        NULL as quantity_servings,
                        FALSE as has_history,
                        f.barcode,
                        f.quantity_grams,
                        f.calories,
                        f.protein,
                        f.carbs,
                        f.fat,
                        f.fiber,
                        f.sugar,
                        f.sodium,
                        f.saturated_fat,
                        f.cholesterol,
                        f.potassium,
                        f.calcium
                    FROM 
                        Foods f 
                    WHERE 
                        f.food_id NOT IN (
                            SELECT DISTINCT food_id 
                            FROM User_Foods 
                            WHERE user_id = @UserId
                        )
                    ORDER BY 
                        name;"

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@UserId", UserId)

                        ' Fill the data table
                        Using reader = Await cmd.ExecuteReaderAsync()
                            While Await reader.ReadAsync()
                                Try
                                    Dim row As DataRow = AllData.NewRow()

                                    If Not reader.IsDBNull(0) Then
                                        row("food_id") = reader.GetInt32(0)
                                    Else
                                        Continue While
                                    End If

                                    If Not reader.IsDBNull(1) Then
                                        row("Name") = reader.GetString(1)
                                    Else
                                        row("Name") = "Unknown Food"
                                    End If

                                    If Not reader.IsDBNull(2) Then
                                        row("Date") = reader.GetDateTime(2)
                                    End If

                                    If Not reader.IsDBNull(3) Then
                                        Try
                                            row("Quantity") = Convert.ToInt32(reader.GetValue(3))
                                        Catch
                                        End Try
                                    End If

                                    If Not reader.IsDBNull(4) Then
                                        row("has_history") = reader.GetBoolean(4)
                                    Else
                                        row("has_history") = False
                                    End If

                                    If Not reader.IsDBNull(5) Then
                                        row("barcode") = reader.GetString(5)
                                    End If

                                    If Not reader.IsDBNull(6) Then
                                        row("quantity_grams") = Convert.ToDecimal(reader.GetValue(6))
                                    End If

                                    If Not reader.IsDBNull(7) Then
                                        row("calories") = Convert.ToDecimal(reader.GetValue(7))
                                    End If

                                    If Not reader.IsDBNull(8) Then
                                        row("protein") = Convert.ToDecimal(reader.GetValue(8))
                                    End If

                                    If Not reader.IsDBNull(9) Then
                                        row("carbs") = Convert.ToDecimal(reader.GetValue(9))
                                    End If

                                    If Not reader.IsDBNull(10) Then
                                        row("fat") = Convert.ToDecimal(reader.GetValue(10))
                                    End If

                                    If Not reader.IsDBNull(11) Then
                                        row("fiber") = Convert.ToDecimal(reader.GetValue(11))
                                    End If

                                    If Not reader.IsDBNull(12) Then
                                        row("sugar") = Convert.ToDecimal(reader.GetValue(12))
                                    End If

                                    If Not reader.IsDBNull(13) Then
                                        row("sodium") = Convert.ToDecimal(reader.GetValue(13))
                                    End If

                                    If Not reader.IsDBNull(14) Then
                                        row("saturated_fat") = Convert.ToDecimal(reader.GetValue(14))
                                    End If

                                    If Not reader.IsDBNull(15) Then
                                        row("cholesterol") = Convert.ToDecimal(reader.GetValue(15))
                                    End If

                                    If Not reader.IsDBNull(16) Then
                                        row("potassium") = Convert.ToDecimal(reader.GetValue(16))
                                    End If

                                    If Not reader.IsDBNull(17) Then
                                        row("calcium") = Convert.ToDecimal(reader.GetValue(17))
                                    End If

                                    AllData.Rows.Add(row)
                                Catch ex As Exception
                                    Debug.WriteLine("Error processing row: " & ex.Message)
                                End Try
                            End While
                        End Using
                    End Using

                    ' Check if this is the first meal log of the day
                    Dim checkCmd As New Npgsql.NpgsqlCommand("SELECT COUNT(*) FROM User_Foods WHERE user_id = @user_id AND date_consumed = CURRENT_DATE", conn)
                    checkCmd.Parameters.AddWithValue("user_id", UserId)
                    mealCount = Convert.ToInt32(checkCmd.ExecuteScalar())
                End Using

                Return

            Catch ex As Exception
                Debug.WriteLine($"Database load attempt {retryCount + 1} failed: {ex.Message}")
                retryCount += 1

                Task.Run(Sub()
                             System.Threading.Thread.Sleep(500 * retryCount)
                         End Sub).Wait()
            End Try
        End While

        Debug.WriteLine("All database load attempts failed")
    End Function

    ' Apply filters to the already-loaded data
    Private Sub ApplyFiltersLocally()
        Me.Cursor = Cursors.WaitCursor

        Try
            If AllData Is Nothing OrElse AllData.Rows.Count = 0 Then
                DataGridView1.DataSource = Nothing
                Me.Cursor = Cursors.Default
                Return
            End If

            Dim searchText As String = SearchTextBox.Text.Trim().ToLower()
            Dim isSearching As Boolean = Not String.IsNullOrEmpty(searchText) AndAlso searchText.Length >= 3

            Dim filteredTable As New DataTable()
            filteredTable = AllData.Clone()

            ' Track food IDs we've already added during search
            Dim addedFoodIds As New HashSet(Of Integer)()

            ' Filter the rows based on criteria
            For Each sourceRow As DataRow In AllData.Rows
                Dim foodId As Integer = Convert.ToInt32(sourceRow("food_id"))
                Dim foodName As String = sourceRow("Name").ToString().ToLower()
                Dim hasHistory As Boolean = CBool(sourceRow("has_history"))

                ' Skip non-consumed foods when not searching
                If Not hasHistory And Not isSearching Then
                    Continue For
                End If

                If isSearching Then
                    ' Include this row only if it matches the search text
                    If Not foodName.Contains(searchText) Then
                        Continue For
                    End If

                    ' Skip if we already added this food (prevents duplicates)
                    If addedFoodIds.Contains(foodId) Then
                        Continue For
                    End If

                    addedFoodIds.Add(foodId)
                    filteredTable.ImportRow(sourceRow)

                    Dim newRow As DataRow = filteredTable.Rows(filteredTable.Rows.Count - 1)

                    ' Set Date and Quantity to null when searching
                    If filteredTable.Columns.Contains("Date") Then
                        newRow("Date") = DBNull.Value
                    End If
                    If filteredTable.Columns.Contains("Quantity") Then
                        newRow("Quantity") = DBNull.Value
                    End If

                ElseIf IsDateFilterActive AndAlso hasHistory Then
                    If sourceRow.IsNull("Date") Then
                        Continue For
                    End If

                    Dim consumedDate As DateTime = CDate(sourceRow("Date"))

                    If consumedDate.Date <> SelectedFilterDate.Date Then
                        Continue For
                    End If

                    filteredTable.ImportRow(sourceRow)
                Else
                    filteredTable.ImportRow(sourceRow)
                End If
            Next

            DataGridView1.DataSource = Nothing
            DataGridView1.Columns.Clear()

            ' Sort the filtered table
            Dim view As New DataView(filteredTable)

            If isSearching Then
                view.Sort = "Name ASC"
            ElseIf filteredTable.Columns.Contains("Date") Then
                view.Sort = "Date DESC"
            End If

            DataGridView1.DataSource = view

            If DataGridView1.Columns.Contains("food_id") Then
                DataGridView1.Columns("food_id").Visible = False
            End If
            If DataGridView1.Columns.Contains("has_history") Then
                DataGridView1.Columns("has_history").Visible = False
            End If
            If DataGridView1.Columns.Contains("barcode") Then
                DataGridView1.Columns("barcode").Visible = False
            End If

            If isSearching Then
                If DataGridView1.Columns.Contains("Date") Then
                    DataGridView1.Columns("Date").Visible = False
                End If
                If DataGridView1.Columns.Contains("Quantity") Then
                    DataGridView1.Columns("Quantity").Visible = False
                End If
            ElseIf DataGridView1.Columns.Contains("Date") Then
                DataGridView1.Columns("Date").DefaultCellStyle.Format = "g"
            End If

            ' Hide all nutrition columns
            If DataGridView1.Columns.Contains("quantity_grams") Then
                DataGridView1.Columns("quantity_grams").Visible = False
            End If
            If DataGridView1.Columns.Contains("calories") Then
                DataGridView1.Columns("calories").Visible = False
            End If
            If DataGridView1.Columns.Contains("protein") Then
                DataGridView1.Columns("protein").Visible = False
            End If
            If DataGridView1.Columns.Contains("carbs") Then
                DataGridView1.Columns("carbs").Visible = False
            End If
            If DataGridView1.Columns.Contains("fat") Then
                DataGridView1.Columns("fat").Visible = False
            End If
            If DataGridView1.Columns.Contains("fiber") Then
                DataGridView1.Columns("fiber").Visible = False
            End If
            If DataGridView1.Columns.Contains("sugar") Then
                DataGridView1.Columns("sugar").Visible = False
            End If
            If DataGridView1.Columns.Contains("sodium") Then
                DataGridView1.Columns("sodium").Visible = False
            End If
            If DataGridView1.Columns.Contains("saturated_fat") Then
                DataGridView1.Columns("saturated_fat").Visible = False
            End If
            If DataGridView1.Columns.Contains("cholesterol") Then
                DataGridView1.Columns("cholesterol").Visible = False
            End If
            If DataGridView1.Columns.Contains("potassium") Then
                DataGridView1.Columns("potassium").Visible = False
            End If
            If DataGridView1.Columns.Contains("calcium") Then
                DataGridView1.Columns("calcium").Visible = False
            End If

            AddActionButtonColumns()

            RemoveHandler DataGridView1.CellClick, AddressOf DataGridView1_CellClick
            AddHandler DataGridView1.CellClick, AddressOf DataGridView1_CellClick

            If isSearching Then
                SearchModeLabel.Text = "Showing all meals matching: " & SearchTextBox.Text.Trim()
                SearchModeLabel.Visible = True
                ClearSearchButton.Visible = True
            Else
                SearchModeLabel.Visible = False
                ClearSearchButton.Visible = False
            End If

        Catch ex As Exception
            Debug.WriteLine("Error applying filters: " & ex.Message)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub AddActionButtonColumns()
        Dim detailsButtonColumn As New DataGridViewButtonColumn()
        Dim addButtonColumn As New DataGridViewButtonColumn()
        Dim deleteButtonColumn As New DataGridViewButtonColumn()

        detailsButtonColumn.Name = "DetailsButton"
        detailsButtonColumn.HeaderText = "Details"
        detailsButtonColumn.Text = "View"
        detailsButtonColumn.UseColumnTextForButtonValue = True

        addButtonColumn.Name = "AddButton"
        addButtonColumn.HeaderText = "Add"
        addButtonColumn.Text = "Add"
        addButtonColumn.UseColumnTextForButtonValue = True

        deleteButtonColumn.Name = "DeleteButton"
        deleteButtonColumn.HeaderText = "Delete"
        deleteButtonColumn.Text = "Delete"
        deleteButtonColumn.UseColumnTextForButtonValue = True

        DataGridView1.Columns.Add(detailsButtonColumn)
        DataGridView1.Columns.Add(addButtonColumn)
        DataGridView1.Columns.Add(deleteButtonColumn)
    End Sub

    Private Sub DataGridView1_CellClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return

        Dim foodId As Integer = Convert.ToInt32(DataGridView1.Rows(e.RowIndex).Cells("food_id").Value)
        Dim foodName As String = DataGridView1.Rows(e.RowIndex).Cells("Name").Value.ToString()

        Dim hasHistory As Boolean = False
        If DataGridView1.Columns.Contains("has_history") Then
            hasHistory = Convert.ToBoolean(DataGridView1.Rows(e.RowIndex).Cells("has_history").Value)
        ElseIf DataGridView1.Columns.Contains("Date") AndAlso DataGridView1.Rows(e.RowIndex).Cells("Date").Value IsNot DBNull.Value Then
            hasHistory = True
        End If

        ' Get date consumed (if available) for specific deletion
        Dim dateConsumed As DateTime? = Nothing
        If DataGridView1.Columns.Contains("Date") AndAlso DataGridView1.Rows(e.RowIndex).Cells("Date").Value IsNot DBNull.Value Then
            dateConsumed = Convert.ToDateTime(DataGridView1.Rows(e.RowIndex).Cells("Date").Value)
        End If

        ' Get quantity servings (if available)
        Dim quantityServing As Integer? = Nothing
        If DataGridView1.Columns.Contains("Quantity") AndAlso DataGridView1.Rows(e.RowIndex).Cells("Quantity").Value IsNot DBNull.Value Then
            quantityServing = Convert.ToInt32(DataGridView1.Rows(e.RowIndex).Cells("Quantity").Value)
        End If

        Dim nutritionInfo As New Dictionary(Of String, Object)()

        nutritionInfo.Add("quantity_grams", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("quantity_grams").Value))
        nutritionInfo.Add("calories", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("calories").Value))
        nutritionInfo.Add("protein", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("protein").Value))
        nutritionInfo.Add("carbs", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("carbs").Value))
        nutritionInfo.Add("fat", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("fat").Value))
        nutritionInfo.Add("fiber", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("fiber").Value))
        nutritionInfo.Add("sugar", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("sugar").Value))
        nutritionInfo.Add("sodium", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("sodium").Value))
        nutritionInfo.Add("saturated_fat", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("saturated_fat").Value))
        nutritionInfo.Add("cholesterol", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("cholesterol").Value))
        nutritionInfo.Add("potassium", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("potassium").Value))
        nutritionInfo.Add("calcium", GetCellValue(DataGridView1.Rows(e.RowIndex).Cells("calcium").Value))

        If e.ColumnIndex = DataGridView1.Columns("DetailsButton").Index Then
            Dim detailsForm As FoodDetails

            If hasHistory AndAlso quantityServing.HasValue AndAlso quantityServing.Value > 0 Then
                detailsForm = New FoodDetails(foodId, quantityServing.Value, foodName, nutritionInfo)
            Else
                detailsForm = New FoodDetails(foodId, 1, foodName, nutritionInfo)
            End If

            detailsForm.ShowDialog()

        ElseIf e.ColumnIndex = DataGridView1.Columns("AddButton").Index Then
            Dim form As New Form()
            form.Text = "Add Meal"
            form.Width = 350
            form.Height = 220
            form.FormBorderStyle = FormBorderStyle.FixedDialog
            form.StartPosition = FormStartPosition.CenterParent
            form.MaximizeBox = False
            form.MinimizeBox = False

            Dim dateLabel As New Label()
            dateLabel.Text = "Date:"
            dateLabel.Location = New Point(20, 20)
            dateLabel.AutoSize = True
            form.Controls.Add(dateLabel)

            Dim datePicker As New DateTimePicker()
            datePicker.Format = DateTimePickerFormat.Short

            If IsDateFilterActive Then
                datePicker.Value = SelectedFilterDate
            Else
                datePicker.Value = DateTime.Today
            End If

            datePicker.Location = New Point(150, 20)
            datePicker.Width = 150
            form.Controls.Add(datePicker)

            Dim qtyLabel As New Label()
            qtyLabel.Text = "Quantity (servings):"
            qtyLabel.Location = New Point(20, 60)
            qtyLabel.AutoSize = True
            form.Controls.Add(qtyLabel)

            Dim qtyNumeric As New NumericUpDown()
            qtyNumeric.Minimum = 1
            qtyNumeric.Maximum = 10
            qtyNumeric.Value = 1
            qtyNumeric.Location = New Point(150, 60)
            qtyNumeric.Width = 150
            form.Controls.Add(qtyNumeric)

            Dim foodNameLabel As New Label()
            foodNameLabel.Text = "Food: " & foodName
            foodNameLabel.Location = New Point(20, 100)
            foodNameLabel.AutoSize = True
            foodNameLabel.Font = New Font(foodNameLabel.Font, FontStyle.Bold)
            form.Controls.Add(foodNameLabel)

            Dim cancelButton As New Button()
            cancelButton.Text = "Cancel"
            cancelButton.DialogResult = DialogResult.Cancel
            cancelButton.Location = New Point(65, 140)
            cancelButton.Width = 100
            form.Controls.Add(cancelButton)
            form.CancelButton = cancelButton

            Dim confirmButton As New Button()
            confirmButton.Text = "Add"
            confirmButton.DialogResult = DialogResult.OK
            confirmButton.Location = New Point(185, 140)
            confirmButton.Width = 100
            form.Controls.Add(confirmButton)
            form.AcceptButton = confirmButton

            If form.ShowDialog() = DialogResult.OK Then
                Dim selectedDate As DateTime = datePicker.Value.Date
                Dim selectedQuantity As Integer = CInt(qtyNumeric.Value)

                Dim existingMeal As Boolean = False
                Dim existingDateTime As DateTime? = Nothing

                For Each row As DataRow In AllData.Rows
                    If CInt(row("food_id")) = foodId AndAlso Not row.IsNull("Date") Then
                        Dim rowDate As DateTime = CDate(row("Date"))
                        If rowDate.Date = selectedDate.Date Then
                            existingMeal = True
                            existingDateTime = rowDate
                            Exit For
                        End If
                    End If
                Next

                If existingMeal And existingDateTime.HasValue Then
                    OptimisticUpdateQuantity(foodId, existingDateTime.Value, selectedQuantity)

                    Dim dateTime As DateTime = existingDateTime.Value
                    RetryQueue.Enqueue(Sub() UpdateQuantityDatabase(foodId, dateTime, selectedQuantity))
                Else
                    OptimisticAddMeal(foodId, foodName, selectedDate, selectedQuantity)

                    If mealCount = 0 Then
                        RetryQueue.Enqueue(Sub() InsertPoints())
                    End If
                    mealCount += 1

                    RetryQueue.Enqueue(Sub() AddMealToUserHistoryDatabase(foodId, foodName, selectedDate, selectedQuantity))
                End If
            End If

        ElseIf e.ColumnIndex = DataGridView1.Columns("DeleteButton").Index Then
            If hasHistory And dateConsumed.HasValue Then
                Dim form As New Form()
                form.Text = "Modify Quantity"
                form.Width = 350
                form.Height = 220
                form.FormBorderStyle = FormBorderStyle.FixedDialog
                form.StartPosition = FormStartPosition.CenterParent
                form.MaximizeBox = False
                form.MinimizeBox = False

                Dim dateLabel As New Label()
                dateLabel.Text = "Date: " & dateConsumed.Value.ToShortDateString()
                dateLabel.Location = New Point(20, 20)
                dateLabel.AutoSize = True
                form.Controls.Add(dateLabel)

                Dim currentQtyLabel As New Label()
                currentQtyLabel.Text = "Current Quantity: " & quantityServing.ToString()
                currentQtyLabel.Location = New Point(20, 50)
                currentQtyLabel.AutoSize = True
                form.Controls.Add(currentQtyLabel)

                Dim newQtyLabel As New Label()
                newQtyLabel.Text = "New Quantity:"
                newQtyLabel.Location = New Point(20, 80)
                newQtyLabel.AutoSize = True
                form.Controls.Add(newQtyLabel)

                Dim qtyNumeric As New NumericUpDown()
                qtyNumeric.Minimum = 0
                qtyNumeric.Maximum = 10
                qtyNumeric.Value = If(quantityServing.HasValue, quantityServing.Value, 0)
                qtyNumeric.Location = New Point(150, 80)
                qtyNumeric.Width = 150
                form.Controls.Add(qtyNumeric)

                Dim noteLabel As New Label()
                noteLabel.Text = "Set to 0 to remove this meal completely."
                noteLabel.Location = New Point(20, 110)
                noteLabel.AutoSize = True
                noteLabel.ForeColor = Color.Gray
                form.Controls.Add(noteLabel)

                Dim cancelButton As New Button()
                cancelButton.Text = "Cancel"
                cancelButton.DialogResult = DialogResult.Cancel
                cancelButton.Location = New Point(65, 140)
                cancelButton.Width = 100
                form.Controls.Add(cancelButton)
                form.CancelButton = cancelButton

                Dim confirmButton As New Button()
                confirmButton.Text = "Update"
                confirmButton.DialogResult = DialogResult.OK
                confirmButton.Location = New Point(185, 140)
                confirmButton.Width = 100
                form.Controls.Add(confirmButton)
                form.AcceptButton = confirmButton

                If form.ShowDialog() = DialogResult.OK Then
                    Dim newQuantity As Integer = CInt(qtyNumeric.Value)

                    If newQuantity = 0 Then
                        OptimisticDeleteMeal(foodId, dateConsumed.Value)

                        Dim mealDate As DateTime = dateConsumed.Value
                        RetryQueue.Enqueue(Sub() DeleteMealCompletelyDatabase(foodId, mealDate))
                    ElseIf newQuantity <> quantityServing Then
                        OptimisticSetQuantity(foodId, dateConsumed.Value, newQuantity)

                        Dim mealDate As DateTime = dateConsumed.Value
                        RetryQueue.Enqueue(Sub() SetQuantityDatabase(foodId, mealDate, newQuantity))
                    End If
                End If
            End If
        End If
    End Sub

    ' Helper function to get cell value or DBNull
    Private Function GetCellValue(value As Object) As Object
        If value Is DBNull.Value Then
            Return Nothing
        Else
            Return value
        End If
    End Function

    Private Sub FilterComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DateFilterComboBox.SelectedIndexChanged
        ' Update filter based on selected option
        FilterMode = DateFilterComboBox.SelectedIndex

        Select Case FilterMode
            Case 0  ' Today's Meals
                SelectedFilterDate = DateTime.Today
                IsDateFilterActive = True
                DateFilterPicker.Enabled = False
                DateFilterLabel.Visible = True

            Case 1  ' Custom Date
                DateFilterPicker.Enabled = True
                SelectedFilterDate = DateFilterPicker.Value.Date
                IsDateFilterActive = True
                DateFilterLabel.Visible = True

            Case 2  ' No Filter
                IsDateFilterActive = False
                DateFilterPicker.Enabled = False
                DateFilterLabel.Visible = True
        End Select

        ApplyFiltersLocally()
    End Sub

    Private Sub DateFilterPicker_ValueChanged(sender As Object, e As EventArgs) Handles DateFilterPicker.ValueChanged
        ' Only update if we're in custom date mode
        If FilterMode = 1 Then
            SelectedFilterDate = DateFilterPicker.Value.Date
            DateFilterLabel.Text = "Showing meals from: " & SelectedFilterDate.ToShortDateString()

            ApplyFiltersLocally()
        End If
    End Sub

    Private Sub BackButton_Click(sender As Object, e As EventArgs) Handles BackButton.Click
        ' Return to the main form
        Me.Hide()
        UserDashboard.Show()
    End Sub

    Private Sub SearchTextBox_TextChanged(sender As Object, e As EventArgs) Handles SearchTextBox.TextChanged
        Dim searchText As String = SearchTextBox.Text.Trim()

        If searchText.Length >= 3 Or searchText.Length = 0 Then
            ApplyFiltersLocally()
        End If
    End Sub

    Private Sub ClearSearchButton_Click(sender As Object, e As EventArgs) Handles ClearSearchButton.Click
        SearchTextBox.Clear()
        ApplyFiltersLocally()
    End Sub

    ' Optimistic UI Updates - immediately update the UI without waiting for database
    Private Sub OptimisticAddMeal(foodId As Integer, foodName As String, targetDate As DateTime, quantity As Integer)
        Try
            ' Keep the data model consistent
            Dim nutritionData As DataRow = Nothing
            For Each row As DataRow In AllData.Rows
                If CInt(row("food_id")) = foodId Then
                    nutritionData = row
                    Exit For
                End If
            Next

            Dim newRow As DataRow = AllData.NewRow()
            newRow("food_id") = foodId
            newRow("Name") = foodName
            newRow("Date") = targetDate
            newRow("Quantity") = quantity
            newRow("has_history") = True

            If nutritionData IsNot Nothing Then
                If Not nutritionData.IsNull("quantity_grams") Then newRow("quantity_grams") = nutritionData("quantity_grams")
                If Not nutritionData.IsNull("calories") Then newRow("calories") = nutritionData("calories")
                If Not nutritionData.IsNull("protein") Then newRow("protein") = nutritionData("protein")
                If Not nutritionData.IsNull("carbs") Then newRow("carbs") = nutritionData("carbs")
                If Not nutritionData.IsNull("fat") Then newRow("fat") = nutritionData("fat")
                If Not nutritionData.IsNull("fiber") Then newRow("fiber") = nutritionData("fiber")
                If Not nutritionData.IsNull("sugar") Then newRow("sugar") = nutritionData("sugar")
                If Not nutritionData.IsNull("sodium") Then newRow("sodium") = nutritionData("sodium")
                If Not nutritionData.IsNull("saturated_fat") Then newRow("saturated_fat") = nutritionData("saturated_fat")
                If Not nutritionData.IsNull("cholesterol") Then newRow("cholesterol") = nutritionData("cholesterol")
                If Not nutritionData.IsNull("potassium") Then newRow("potassium") = nutritionData("potassium")
                If Not nutritionData.IsNull("calcium") Then newRow("calcium") = nutritionData("calcium")
            End If

            AllData.Rows.Add(newRow)

            For i As Integer = AllData.Rows.Count - 1 To 0 Step -1
                Dim row As DataRow = AllData.Rows(i)
                If CInt(row("food_id")) = foodId And CBool(row("has_history")) = False Then
                    AllData.Rows.RemoveAt(i)
                End If
            Next

            ApplyFiltersLocally()

            ShowToast($"Added '{foodName}' ({quantity} servings)")

        Catch ex As Exception
            Debug.WriteLine("Error in optimistic update: " & ex.Message)
        End Try
    End Sub

    Private Sub OptimisticUpdateQuantity(foodId As Integer, dateConsumed As DateTime, newQuantity As Integer)
        Try
            ' Update our local cache - find the row for this meal and update quantity
            For Each row As DataRow In AllData.Rows
                If CInt(row("food_id")) <> foodId Then Continue For

                If row.IsNull("Date") Then Continue For

                Dim rowDate As DateTime = CDate(row("Date"))
                If rowDate = dateConsumed Then
                    row("Quantity") = newQuantity
                    Exit For
                End If
            Next

            ApplyFiltersLocally()

            ShowToast($"Updated meal quantity to {newQuantity}")

        Catch ex As Exception
            Debug.WriteLine("Error in optimistic update: " & ex.Message)
        End Try
    End Sub

    Private Sub OptimisticSetQuantity(foodId As Integer, dateConsumed As DateTime, newQuantity As Integer)
        Try
            ' Update our local cache - find the row for this meal and set quantity
            For Each row As DataRow In AllData.Rows
                If CInt(row("food_id")) <> foodId Then Continue For

                If row.IsNull("Date") Then Continue For

                Dim rowDate As DateTime = CDate(row("Date"))
                If rowDate = dateConsumed Then
                    row("Quantity") = newQuantity
                    Exit For
                End If
            Next

            ApplyFiltersLocally()

            ShowToast($"Set meal quantity to {newQuantity}")

        Catch ex As Exception
            Debug.WriteLine("Error in optimistic update: " & ex.Message)
        End Try
    End Sub

    Private Sub OptimisticDeleteMeal(foodId As Integer, dateConsumed As DateTime)
        Try
            Dim foodName As String = ""

            ' Update our local cache - remove the deleted row
            For i As Integer = AllData.Rows.Count - 1 To 0 Step -1
                Dim row As DataRow = AllData.Rows(i)

                If CInt(row("food_id")) <> foodId Then Continue For

                If row.IsNull("Date") Then Continue For

                Dim rowDate As DateTime = CDate(row("Date"))
                If rowDate = dateConsumed Then
                    foodName = row("Name").ToString()
                    AllData.Rows.RemoveAt(i)
                    Exit For
                End If
            Next

            Dim hasRemainingEntries As Boolean = False
            For Each row As DataRow In AllData.Rows
                If CInt(row("food_id")) = foodId And CBool(row("has_history")) = True Then
                    hasRemainingEntries = True
                    Exit For
                End If
            Next

            ApplyFiltersLocally()

            If Not String.IsNullOrEmpty(foodName) Then
                ShowToast($"Removed '{foodName}' from your meals")
            Else
                ShowToast("Removed meal")
            End If

        Catch ex As Exception
            Debug.WriteLine("Error in optimistic update: " & ex.Message)
        End Try
    End Sub

    ' Show a non-blocking toast message
    Private Sub ShowToast(message As String)
        Dim toast As New Form()
        toast.Text = ""
        toast.FormBorderStyle = FormBorderStyle.None
        toast.BackColor = Color.FromArgb(60, 60, 60)
        toast.ForeColor = Color.White
        toast.Opacity = 0.9
        toast.ShowInTaskbar = False
        toast.TopMost = True
        toast.StartPosition = FormStartPosition.Manual

        toast.Size = New Size(300, 50)
        toast.Location = New Point(
            Me.Location.X + Me.Width - toast.Width - 20,
            Me.Location.Y + Me.Height - toast.Height - 20)

        Dim label As New Label()
        label.Text = message
        label.AutoSize = False
        label.TextAlign = ContentAlignment.MiddleCenter
        label.Dock = DockStyle.Fill
        label.Font = New Font(label.Font.FontFamily, 10)
        toast.Controls.Add(label)

        Dim timer As New System.Windows.Forms.Timer()
        timer.Interval = 2000
        AddHandler timer.Tick, Sub(s, e)
                                   timer.Stop()
                                   toast.Close()
                                   timer.Dispose()
                               End Sub

        toast.Show()
        timer.Start()
    End Sub

    ' Database operations - these run in the background and don't block the UI
    Private Async Function AddMealToUserHistoryDatabase(foodId As Integer, foodName As String, targetDate As DateTime, quantity As Integer) As Task
        Dim connString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;"
        Dim maxRetries As Integer = 5
        Dim retryCount As Integer = 0

        While retryCount < maxRetries
            Try
                Using conn As New NpgsqlConnection(connString)
                    Await conn.OpenAsync()

                    ' Award points if it's the first meal of the day
                    If mealCount = 0 Then
                        Dim insertPointsCmd As New NpgsqlCommand("INSERT INTO Points (user_id, points, date_recorded) VALUES (@user_id, 50, CURRENT_DATE)", conn)
                        insertPointsCmd.Parameters.AddWithValue("user_id", UserId)
                        Await insertPointsCmd.ExecuteNonQueryAsync()
                        MealLogtrue = True
                    End If

                    Dim query As String = "INSERT INTO User_Foods (user_id, food_id, date_consumed, quantity_servings) VALUES (@UserId, @FoodId, @TargetDate, @Quantity);"

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@UserId", UserId)
                        cmd.Parameters.AddWithValue("@FoodId", foodId)
                        cmd.Parameters.AddWithValue("@TargetDate", targetDate)
                        cmd.Parameters.AddWithValue("@Quantity", quantity)

                        Await cmd.ExecuteNonQueryAsync()
                    End Using

                End Using
                mealCount += 1
                ' Operation successful, exit retry loop
                Return

            Catch ex As Exception
                Debug.WriteLine($"Database operation attempt {retryCount + 1} failed: {ex.Message}")
                retryCount += 1

                Task.Run(Sub()
                             System.Threading.Thread.Sleep(500 * retryCount)
                         End Sub).Wait()
            End Try
        End While

        ' If we get here, all retries failed
        Debug.WriteLine("All database operation attempts failed for AddMealToUserHistoryDatabase")
    End Function

    Private Async Function UpdateQuantityDatabase(foodId As Integer, dateConsumed As DateTime, newQuantity As Integer) As Task
        Dim connString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;"
        Dim maxRetries As Integer = 5
        Dim retryCount As Integer = 0

        While retryCount < maxRetries
            Try
                Using conn As New NpgsqlConnection(connString)
                    Await conn.OpenAsync()

                    Dim query As String = "UPDATE User_Foods SET quantity_servings = @NewQuantity WHERE food_id = @FoodId AND user_id = @UserId AND date_consumed = @DateConsumed;"

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@UserId", UserId)
                        cmd.Parameters.AddWithValue("@FoodId", foodId)
                        cmd.Parameters.AddWithValue("@DateConsumed", dateConsumed)
                        cmd.Parameters.AddWithValue("@NewQuantity", newQuantity)

                        Await cmd.ExecuteNonQueryAsync()
                    End Using
                End Using

                ' Operation successful, exit retry loop
                Return

            Catch ex As Exception
                Debug.WriteLine($"Database operation attempt {retryCount + 1} failed: {ex.Message}")
                retryCount += 1

                Task.Run(Sub()
                             System.Threading.Thread.Sleep(500 * retryCount)
                         End Sub).Wait()
            End Try
        End While

        ' If we get here, all retries failed
        Debug.WriteLine("All database operation attempts failed for UpdateQuantityDatabase")
    End Function

    Private Async Function SetQuantityDatabase(foodId As Integer, dateConsumed As DateTime, newQuantity As Integer) As Task
        Dim connString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;"
        Dim maxRetries As Integer = 5
        Dim retryCount As Integer = 0

        While retryCount < maxRetries
            Try
                Using conn As New NpgsqlConnection(connString)
                    Await conn.OpenAsync()

                    Dim query As String = "UPDATE User_Foods SET quantity_servings = @NewQuantity WHERE food_id = @FoodId AND user_id = @UserId AND date_consumed = @DateConsumed;"

                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@UserId", UserId)
                        cmd.Parameters.AddWithValue("@FoodId", foodId)
                        cmd.Parameters.AddWithValue("@DateConsumed", dateConsumed)
                        cmd.Parameters.AddWithValue("@NewQuantity", newQuantity)

                        Await cmd.ExecuteNonQueryAsync()
                    End Using

                    Dim cleanQuery As String = "DELETE FROM User_Foods WHERE (quantity_servings IS NULL OR quantity_servings <= 0);"
                    Using cmd As New NpgsqlCommand(cleanQuery, conn)
                        Await cmd.ExecuteNonQueryAsync()
                    End Using
                End Using

                ' Operation successful, exit retry loop
                Return

            Catch ex As Exception
                Debug.WriteLine($"Database operation attempt {retryCount + 1} failed: {ex.Message}")
                retryCount += 1

                Task.Run(Sub()
                             System.Threading.Thread.Sleep(500 * retryCount)
                         End Sub).Wait()
            End Try
        End While

        ' If we get here, all retries failed
        Debug.WriteLine("All database operation attempts failed for SetQuantityDatabase")
    End Function

    Private Async Function DeleteMealCompletelyDatabase(foodId As Integer, dateConsumed As DateTime) As Task
        Dim connString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;"
        Dim maxRetries As Integer = 5
        Dim retryCount As Integer = 0

        While retryCount < maxRetries
            Try
                Dim addNonHistoryEntry As Boolean = False
                Dim foodName As String = ""
                Dim nutritionInfo As New Dictionary(Of String, Object)()

                Using conn As New NpgsqlConnection(connString)
                    Await conn.OpenAsync()

                    Dim query As String = "DELETE FROM User_Foods WHERE user_id = @UserId AND food_id = @FoodId AND date_consumed = @DateConsumed;"
                    Using cmd As New NpgsqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@UserId", UserId)
                        cmd.Parameters.AddWithValue("@FoodId", foodId)
                        cmd.Parameters.AddWithValue("@DateConsumed", dateConsumed)

                        Await cmd.ExecuteNonQueryAsync()
                    End Using

                    ' Keep the local data model consistent with the database
                    Dim checkQuery As String = "SELECT COUNT(*) FROM User_Foods WHERE food_id = @FoodId AND user_id = @UserId;"
                    Using cmd As New NpgsqlCommand(checkQuery, conn)
                        cmd.Parameters.AddWithValue("@UserId", UserId)
                        cmd.Parameters.AddWithValue("@FoodId", foodId)

                        Dim count As Integer = Convert.ToInt32(Await cmd.ExecuteScalarAsync())

                        If count = 0 Then
                            addNonHistoryEntry = True

                            Dim foodQuery As String = "SELECT name, quantity_grams, calories, protein, carbs, fat, fiber, sugar, sodium, saturated_fat, cholesterol, potassium, calcium FROM Foods WHERE food_id = @FoodId;"
                            Using foodCmd As New NpgsqlCommand(foodQuery, conn)
                                foodCmd.Parameters.AddWithValue("@FoodId", foodId)

                                Using reader = Await foodCmd.ExecuteReaderAsync()
                                    If Await reader.ReadAsync() Then
                                        foodName = reader("name").ToString()

                                        If Not reader.IsDBNull(reader.GetOrdinal("quantity_grams")) Then
                                            nutritionInfo("quantity_grams") = Convert.ToDecimal(reader("quantity_grams"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("calories")) Then
                                            nutritionInfo("calories") = Convert.ToDecimal(reader("calories"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("protein")) Then
                                            nutritionInfo("protein") = Convert.ToDecimal(reader("protein"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("carbs")) Then
                                            nutritionInfo("carbs") = Convert.ToDecimal(reader("carbs"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("fat")) Then
                                            nutritionInfo("fat") = Convert.ToDecimal(reader("fat"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("fiber")) Then
                                            nutritionInfo("fiber") = Convert.ToDecimal(reader("fiber"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("sugar")) Then
                                            nutritionInfo("sugar") = Convert.ToDecimal(reader("sugar"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("sodium")) Then
                                            nutritionInfo("sodium") = Convert.ToDecimal(reader("sodium"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("saturated_fat")) Then
                                            nutritionInfo("saturated_fat") = Convert.ToDecimal(reader("saturated_fat"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("cholesterol")) Then
                                            nutritionInfo("cholesterol") = Convert.ToDecimal(reader("cholesterol"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("potassium")) Then
                                            nutritionInfo("potassium") = Convert.ToDecimal(reader("potassium"))
                                        End If
                                        If Not reader.IsDBNull(reader.GetOrdinal("calcium")) Then
                                            nutritionInfo("calcium") = Convert.ToDecimal(reader("calcium"))
                                        End If
                                    End If
                                End Using
                            End Using
                        End If
                    End Using
                End Using

                If addNonHistoryEntry AndAlso Not String.IsNullOrEmpty(foodName) Then
                    Me.Invoke(Sub()
                                  Dim entryExists As Boolean = False
                                  For Each row As DataRow In AllData.Rows
                                      If CInt(row("food_id")) = foodId AndAlso CBool(row("has_history")) = False Then
                                          entryExists = True
                                          Exit For
                                      End If
                                  Next

                                  If Not entryExists Then
                                      Dim newRow As DataRow = AllData.NewRow()
                                      newRow("food_id") = foodId
                                      newRow("Name") = foodName
                                      newRow("has_history") = False

                                      For Each kvp In nutritionInfo
                                          If kvp.Value IsNot Nothing Then
                                              newRow(kvp.Key) = kvp.Value
                                          End If
                                      Next

                                      AllData.Rows.Add(newRow)
                                      ApplyFiltersLocally()
                                  End If
                              End Sub)
                End If

                ' Operation successful, exit retry loop
                Return

            Catch ex As Exception
                Debug.WriteLine($"Database operation attempt {retryCount + 1} failed: {ex.Message}")
                retryCount += 1

                Task.Run(Sub()
                             System.Threading.Thread.Sleep(500 * retryCount)
                         End Sub).Wait()
            End Try
        End While

        ' If we get here, all retries failed
        Debug.WriteLine("All database operation attempts failed for DeleteMealCompletelyDatabase")
    End Function


    Private Async Function InsertPoints() As Task
        Dim connString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;"
        Dim maxRetries As Integer = 5
        Dim retryCount As Integer = 0

        While retryCount < maxRetries
            Try
                Using conn As New NpgsqlConnection(connString)
                    Await conn.OpenAsync()

                    Dim insertPointsCmd As New NpgsqlCommand("INSERT INTO Points (user_id, points, date_recorded) VALUES (@user_id, 50, CURRENT_DATE)", conn)
                    insertPointsCmd.Parameters.AddWithValue("user_id", UserId)
                    Await insertPointsCmd.ExecuteNonQueryAsync()
                    MealLogtrue = True

                End Using
                ' Operation successful, exit retry loop
                Return

            Catch ex As Exception
                Debug.WriteLine($"Database operation attempt {retryCount + 1} failed: {ex.Message}")
                retryCount += 1

                Task.Run(Sub()
                             System.Threading.Thread.Sleep(500 * retryCount)
                         End Sub).Wait()
            End Try
        End While

        ' If we get here, all retries failed
        Debug.WriteLine("All database operation attempts failed for InsertPoints")
    End Function

    Private Sub barcodeButton_Click(sender As Object, e As EventArgs) Handles barcodeButton.Click
        barcodeButton.Enabled = False

        Try
            ' Path to your PyInstaller-bundled executable
            Dim executablePath As String = Path.Combine(Application.StartupPath, "..\..\dist\webcam.exe")
            Debug.WriteLine($"Executable path: {executablePath}")

            ' Create and configure process
            Dim process As New Process()
            process.StartInfo.FileName = executablePath
            process.StartInfo.Arguments = "--vb-return"
            process.StartInfo.UseShellExecute = False
            process.StartInfo.RedirectStandardOutput = True
            process.StartInfo.CreateNoWindow = True

            ' Start process and read output synchronously
            process.Start()

            ' Read the output synchronously - this is simpler and often more reliable
            Dim output As String = process.StandardOutput.ReadToEnd()

            ' Wait for the process to exit
            process.WaitForExit()

            If Not output.Contains("BARCODE_RESULT") Then
                MessageBox.Show("No output received from barcode scanner",
                           "No Output", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Extract the last line which should contain the barcode
            Dim lines() As String = output.Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
            Dim barcode As String = ""

            If lines.Length > 0 Then
                Dim lastLine = lines(lines.Length - 1).Trim()

                ' Check if the last line contains the prefix and remove it
                If lastLine.StartsWith("BARCODE_RESULT:") Then
                    barcode = lastLine.Substring("BARCODE_RESULT:".Length)
                Else
                    barcode = lastLine
                End If

                Debug.WriteLine($"Extracted barcode: {barcode}")
                ProcessBarcodeResult(barcode)
            Else
                MessageBox.Show("No barcode data found in the output",
                   "No Barcode", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

        Catch ex As Exception
            Debug.WriteLine($"Error: {ex.Message}")
            Debug.WriteLine($"Stack trace: {ex.StackTrace}")
            MessageBox.Show($"An error occurred: {ex.Message}",
                       "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            barcodeButton.Enabled = True
        End Try
    End Sub

    ' Process a successfully scanned barcode
    Private Sub ProcessBarcodeResult(barcode As String)
        Try
            Dim foundFood As Boolean = False
            Dim foodId As Integer = 0
            Dim foodName As String = ""

            For Each row As DataRow In AllData.Rows
                If Not row.Table.Columns.Contains("barcode") Then Continue For

                If row.IsNull("barcode") Then Continue For

                If row("barcode").ToString() = barcode Then
                    foodId = CInt(row("food_id"))
                    foodName = row("Name").ToString()
                    foundFood = True
                    Exit For
                End If
            Next

            If foundFood Then
                If MessageBox.Show($"Found: {foodName}" & Environment.NewLine &
                    "Would you like to add this to today's meals?",
                    "Food Found", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) = DialogResult.Yes Then

                    Dim quantityForm As New Form()
                    quantityForm.Text = "Select Quantity"
                    quantityForm.Width = 300
                    quantityForm.Height = 150
                    quantityForm.StartPosition = FormStartPosition.CenterParent
                    quantityForm.FormBorderStyle = FormBorderStyle.FixedDialog
                    quantityForm.MaximizeBox = False
                    quantityForm.MinimizeBox = False

                    Dim label As New Label()
                    label.Text = "Quantity (servings):"
                    label.Location = New Point(20, 20)
                    label.AutoSize = True
                    quantityForm.Controls.Add(label)

                    Dim numericUpDown As New NumericUpDown()
                    numericUpDown.Minimum = 1
                    numericUpDown.Maximum = 10
                    numericUpDown.Value = 1
                    numericUpDown.Location = New Point(150, 20)
                    numericUpDown.Width = 100
                    quantityForm.Controls.Add(numericUpDown)

                    Dim okButton As New Button()
                    okButton.Text = "OK"
                    okButton.DialogResult = DialogResult.OK
                    okButton.Location = New Point(170, 70)
                    quantityForm.Controls.Add(okButton)
                    quantityForm.AcceptButton = okButton

                    Dim cancelButton As New Button()
                    cancelButton.Text = "Cancel"
                    cancelButton.DialogResult = DialogResult.Cancel
                    cancelButton.Location = New Point(70, 70)
                    quantityForm.Controls.Add(cancelButton)
                    quantityForm.CancelButton = cancelButton

                    If quantityForm.ShowDialog() = DialogResult.OK Then
                        Dim quantity As Integer = CInt(numericUpDown.Value)

                        ' Use OptimisticAddMeal for immediate UI update
                        OptimisticAddMeal(foodId, foodName, DateTime.Today, quantity)

                        ' Queue the actual database operation with RetryQueue
                        RetryQueue.Enqueue(Sub() AddMealToUserHistoryDatabase(foodId, foodName, DateTime.Today, quantity))

                        If mealCount = 0 Then
                            RetryQueue.Enqueue(Sub() InsertPoints())
                        End If
                        mealCount += 1

                        ShowToast($"Added '{foodName}' ({quantity} servings)")
                    End If
                End If
            Else
                MessageBox.Show($"No meal was found matching barcode: {barcode}",
                        "Barcode Not Found", MessageBoxButtons.OK,
                        MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            Debug.WriteLine($"Error processing barcode: {ex.Message}")
            MessageBox.Show($"Error processing barcode: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
