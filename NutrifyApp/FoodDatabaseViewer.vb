Imports Microsoft.VisualBasic.ApplicationServices
Imports Npgsql
Imports System.Windows.Forms
Imports System.Threading.Tasks
Imports System.Diagnostics
Imports System.Data
Imports System.Collections.Generic
Imports System.Threading

Public Class FoodDatabaseViewer
    ' Colors
    Private PrimaryColor As Color = Color.LimeGreen
    Private ButtonColor As Color = Color.LimeGreen
    Private BackButtonColor As Color = Color.DarkRed
    Private ErrorColor As Color = Color.Red

    ' UI Controls
    Private WithEvents lblTitle As Label
    Private WithEvents lblSearch As Label
    Private WithEvents txtSearch As TextBox
    Private WithEvents cboSearchType As ComboBox
    Private WithEvents cmdSearch As Button
    Private WithEvents cmdClearSearch As Button
    Private WithEvents lblStatus As Label
    Private WithEvents dataGridFoods As DataGridView
    Private WithEvents cmdBack As Button
    Private WithEvents cmdAddFood As Button
    Private WithEvents cmdEditFood As Button

    ' Data
    Private AllFoodsData As DataTable = Nothing
    Private FilteredData As DataView = Nothing
    Private DatabaseOperationInProgress As Boolean = False
    Private CancellationSource As New CancellationTokenSource()

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent1()

        ' Add any initialization after the InitializeComponent() call.
        InitializeUI()
        LoadAllFoodsFromDatabase()
    End Sub

    Private Sub InitializeComponent1()
        Me.Text = "Food Database Viewer"
        Me.Size = New Size(950, 670)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(30, 30, 30)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
    End Sub

    Private Sub InitializeUI()
        Me.Controls.Clear()

        ' === Title Label ===
        lblTitle = New Label With {
            .Text = "Food Database",
            .Font = New Font("Segoe UI", 20, FontStyle.Bold),
            .ForeColor = PrimaryColor,
            .AutoSize = True,
            .Location = New Point(30, 20)
        }
        Me.Controls.Add(lblTitle)

        ' === Search Type Label ===
        lblSearch = New Label With {
            .Text = "Search By:",
            .Font = New Font("Segoe UI", 12),
            .ForeColor = Color.White,
            .Location = New Point(30, 80),
            .AutoSize = True
        }
        Me.Controls.Add(lblSearch)

        ' === Search Type ComboBox ===
        cboSearchType = New ComboBox With {
            .Font = New Font("Segoe UI", 11),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Location = New Point(130, 76),
            .Width = 120
        }
        cboSearchType.Items.AddRange(New String() {"Name", "Barcode"})
        cboSearchType.SelectedIndex = 0  ' Default to "Name"
        Me.Controls.Add(cboSearchType)

        ' === Search TextBox ===
        txtSearch = New TextBox With {
            .Font = New Font("Segoe UI", 11),
            .Location = New Point(260, 76),
            .Width = 300
        }
        Me.Controls.Add(txtSearch)

        ' === Search Button ===
        cmdSearch = New Button With {
            .Text = "Search",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(570, 75),
            .Size = New Size(100, 32)
        }
        Me.Controls.Add(cmdSearch)

        ' === Clear Search Button ===
        cmdClearSearch = New Button With {
            .Text = "Clear",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = BackButtonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(680, 75),
            .Size = New Size(100, 32),
            .Visible = False
        }
        Me.Controls.Add(cmdClearSearch)

        ' === Status Label ===
        lblStatus = New Label With {
            .Font = New Font("Segoe UI", 11),
            .ForeColor = Color.White,
            .Location = New Point(30, 130),
            .AutoSize = True,
            .Text = "Loading food database..."
        }
        Me.Controls.Add(lblStatus)

        ' === DataGridView for Foods ===
        dataGridFoods = New DataGridView With {
            .Location = New Point(30, 160),
            .Size = New Size(880, 400),
            .BackgroundColor = Color.FromArgb(45, 45, 45),
            .BorderStyle = BorderStyle.None,
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            .RowHeadersVisible = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .MultiSelect = False,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False
        }

        ' Configure grid appearance
        dataGridFoods.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45)
        dataGridFoods.DefaultCellStyle.ForeColor = Color.White
        dataGridFoods.DefaultCellStyle.SelectionBackColor = PrimaryColor
        dataGridFoods.DefaultCellStyle.SelectionForeColor = Color.White
        dataGridFoods.DefaultCellStyle.Font = New Font("Segoe UI", 10)

        dataGridFoods.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(55, 55, 55)
        dataGridFoods.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        dataGridFoods.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        dataGridFoods.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        dataGridFoods.ColumnHeadersHeight = 40

        dataGridFoods.EnableHeadersVisualStyles = False
        Me.Controls.Add(dataGridFoods)

        ' === Back Button ===
        cmdBack = New Button With {
            .Text = "← Back",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = BackButtonColor,
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(30, 580),
            .Size = New Size(100, 32)
        }
        Me.Controls.Add(cmdBack)

        ' === Add Food Button ===
        cmdAddFood = New Button With {
            .Text = "Add New Food",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(690, 580),
            .Size = New Size(120, 32)
        }
        Me.Controls.Add(cmdAddFood)

        ' === Edit Food Button ===
        cmdEditFood = New Button With {
            .Text = "Edit Food",
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = ButtonColor,
            .ForeColor = Color.Black,
            .FlatStyle = FlatStyle.Flat,
            .Location = New Point(820, 580),
            .Size = New Size(90, 32)
        }
        Me.Controls.Add(cmdEditFood)

        ' Add event handlers
        AddHandler cmdSearch.Click, AddressOf cmdSearch_Click
        AddHandler cmdClearSearch.Click, AddressOf cmdClearSearch_Click
        AddHandler cmdBack.Click, AddressOf cmdBack_Click
        AddHandler cmdAddFood.Click, AddressOf cmdAddFood_Click
        AddHandler cmdEditFood.Click, AddressOf cmdEditFood_Click
        AddHandler txtSearch.TextChanged, AddressOf txtSearch_TextChanged
    End Sub

    Private Sub FoodDatabaseViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Any additional load operations
    End Sub

    Private Sub FoodDatabaseViewer_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        CancellationSource.Cancel()
    End Sub

    Private Async Sub LoadAllFoodsFromDatabase()
        Me.Cursor = Cursors.WaitCursor
        lblStatus.Text = "Loading food database..."
        lblStatus.Visible = True

        Try
            ' Create our data table with the correct schema
            AllFoodsData = New DataTable()
            AllFoodsData.Columns.Add("food_id", GetType(Integer))
            AllFoodsData.Columns.Add("name", GetType(String))
            AllFoodsData.Columns.Add("barcode", GetType(String))
            AllFoodsData.Columns.Add("quantity_grams", GetType(Integer))
            AllFoodsData.Columns.Add("calories", GetType(Integer))
            AllFoodsData.Columns.Add("protein", GetType(Decimal))
            AllFoodsData.Columns.Add("carbs", GetType(Decimal))
            AllFoodsData.Columns.Add("fat", GetType(Decimal))
            AllFoodsData.Columns.Add("fiber", GetType(Decimal))
            AllFoodsData.Columns.Add("sugar", GetType(Decimal))
            AllFoodsData.Columns.Add("sodium", GetType(Decimal))
            AllFoodsData.Columns.Add("saturated_fat", GetType(Decimal))
            AllFoodsData.Columns.Add("cholesterol", GetType(Decimal))
            AllFoodsData.Columns.Add("potassium", GetType(Decimal))
            AllFoodsData.Columns.Add("calcium", GetType(Decimal))

            ' Make columns nullable where appropriate
            AllFoodsData.Columns("barcode").AllowDBNull = True
            AllFoodsData.Columns("quantity_grams").AllowDBNull = True
            AllFoodsData.Columns("calories").AllowDBNull = True
            AllFoodsData.Columns("protein").AllowDBNull = True
            AllFoodsData.Columns("carbs").AllowDBNull = True
            AllFoodsData.Columns("fat").AllowDBNull = True
            AllFoodsData.Columns("fiber").AllowDBNull = True
            AllFoodsData.Columns("sugar").AllowDBNull = True
            AllFoodsData.Columns("sodium").AllowDBNull = True
            AllFoodsData.Columns("saturated_fat").AllowDBNull = True
            AllFoodsData.Columns("cholesterol").AllowDBNull = True
            AllFoodsData.Columns("potassium").AllowDBNull = True
            AllFoodsData.Columns("calcium").AllowDBNull = True

            ' Run the database query in a background thread
            Await Task.Run(Function() LoadFoodsFromDatabaseAsync())

            ' Now that we have all data, bind it to the grid
            DisplayFoodsData(AllFoodsData)

            lblStatus.Text = $"Found {AllFoodsData.Rows.Count} foods in the database."

        Catch ex As Exception
            Debug.WriteLine("Error loading data: " & ex.Message)
            lblStatus.Text = "Error loading food database. Please try again later."
            lblStatus.ForeColor = ErrorColor
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Async Function LoadFoodsFromDatabaseAsync() As Task
        Dim connString As String = "Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yjcctnidaceciwihhjgg;Password=W.vqW@+DPSh%tk5;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;"
        Dim maxRetries As Integer = 3
        Dim retryCount As Integer = 0

        While retryCount < maxRetries
            Try
                Using conn As New NpgsqlConnection(connString)
                    Await conn.OpenAsync()

                    Dim query As String = "SELECT * FROM Foods ORDER BY name ASC;"

                    Using cmd As New NpgsqlCommand(query, conn)
                        Using reader = Await cmd.ExecuteReaderAsync()
                            While Await reader.ReadAsync()
                                Try
                                    Dim row As DataRow = AllFoodsData.NewRow()

                                    ' Get food_id (should never be null)
                                    If Not reader.IsDBNull(reader.GetOrdinal("food_id")) Then
                                        row("food_id") = reader.GetInt32(reader.GetOrdinal("food_id"))
                                    Else
                                        Continue While
                                    End If

                                    ' Get name (should never be null)
                                    If Not reader.IsDBNull(reader.GetOrdinal("name")) Then
                                        row("name") = reader.GetString(reader.GetOrdinal("name"))
                                    Else
                                        row("name") = "Unknown Food"
                                    End If

                                    ' Get barcode (can be null)
                                    If Not reader.IsDBNull(reader.GetOrdinal("barcode")) Then
                                        row("barcode") = reader.GetString(reader.GetOrdinal("barcode"))
                                    End If

                                    ' Get nutrition information (all can be null)
                                    If Not reader.IsDBNull(reader.GetOrdinal("quantity_grams")) Then
                                        row("quantity_grams") = reader.GetInt32(reader.GetOrdinal("quantity_grams"))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("calories")) Then
                                        row("calories") = reader.GetInt32(reader.GetOrdinal("calories"))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("protein")) Then
                                        row("protein") = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("protein")))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("carbs")) Then
                                        row("carbs") = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("carbs")))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("fat")) Then
                                        row("fat") = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("fat")))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("fiber")) Then
                                        row("fiber") = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("fiber")))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("sugar")) Then
                                        row("sugar") = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("sugar")))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("sodium")) Then
                                        row("sodium") = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("sodium")))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("saturated_fat")) Then
                                        row("saturated_fat") = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("saturated_fat")))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("cholesterol")) Then
                                        row("cholesterol") = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("cholesterol")))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("potassium")) Then
                                        row("potassium") = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("potassium")))
                                    End If

                                    If Not reader.IsDBNull(reader.GetOrdinal("calcium")) Then
                                        row("calcium") = Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("calcium")))
                                    End If

                                    AllFoodsData.Rows.Add(row)
                                Catch ex As Exception
                                    Debug.WriteLine("Error processing row: " & ex.Message)
                                End Try
                            End While
                        End Using
                    End Using
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

    Private Sub DisplayFoodsData(dataTable As DataTable)
        Try
            dataGridFoods.DataSource = Nothing
            dataGridFoods.Columns.Clear()

            If dataTable Is Nothing OrElse dataTable.Rows.Count = 0 Then
                lblStatus.Text = "No foods found in the database."
                Return
            End If

            ' Set the data source
            FilteredData = New DataView(dataTable)
            FilteredData.Sort = "name ASC"
            dataGridFoods.DataSource = FilteredData

            ' Configure the columns
            dataGridFoods.Columns("food_id").Visible = False
            dataGridFoods.Columns("name").HeaderText = "Food Name"
            dataGridFoods.Columns("name").Width = 200
            dataGridFoods.Columns("barcode").HeaderText = "Barcode"
            dataGridFoods.Columns("barcode").Width = 100
            dataGridFoods.Columns("quantity_grams").HeaderText = "Serving (g)"
            dataGridFoods.Columns("quantity_grams").Width = 80
            dataGridFoods.Columns("calories").HeaderText = "Calories"
            dataGridFoods.Columns("calories").Width = 60
            dataGridFoods.Columns("protein").HeaderText = "Protein (g)"
            dataGridFoods.Columns("protein").Width = 70
            dataGridFoods.Columns("carbs").HeaderText = "Carbs (g)"
            dataGridFoods.Columns("carbs").Width = 70
            dataGridFoods.Columns("fat").HeaderText = "Fat (g)"
            dataGridFoods.Columns("fat").Width = 70
            dataGridFoods.Columns("fiber").HeaderText = "Fiber (g)"
            dataGridFoods.Columns("fiber").Width = 70
            dataGridFoods.Columns("sugar").HeaderText = "Sugar (g)"
            dataGridFoods.Columns("sugar").Width = 70
            dataGridFoods.Columns("sodium").HeaderText = "Sodium (mg)"
            dataGridFoods.Columns("sodium").Width = 90
            dataGridFoods.Columns("saturated_fat").HeaderText = "Sat. Fat (g)"
            dataGridFoods.Columns("saturated_fat").Width = 80
            dataGridFoods.Columns("cholesterol").HeaderText = "Cholest. (mg)"
            dataGridFoods.Columns("cholesterol").Width = 90
            dataGridFoods.Columns("potassium").HeaderText = "Potass. (mg)"
            dataGridFoods.Columns("potassium").Width = 90
            dataGridFoods.Columns("calcium").HeaderText = "Calcium (mg)"
            dataGridFoods.Columns("calcium").Width = 90

            ' Format decimal columns
            For Each column As DataGridViewColumn In dataGridFoods.Columns
                If column.Name = "protein" OrElse column.Name = "carbs" OrElse
                   column.Name = "fat" OrElse column.Name = "fiber" OrElse
                   column.Name = "sugar" OrElse column.Name = "saturated_fat" Then
                    column.DefaultCellStyle.Format = "0.0"
                ElseIf column.Name = "sodium" OrElse column.Name = "cholesterol" OrElse
                       column.Name = "potassium" OrElse column.Name = "calcium" Then
                    column.DefaultCellStyle.Format = "0"
                End If
            Next

            ' Finish configuration
            dataGridFoods.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None


        Catch ex As Exception
            Debug.WriteLine("Error displaying data: " & ex.Message)
            lblStatus.Text = "Error displaying food data."
            lblStatus.ForeColor = ErrorColor
        End Try
    End Sub

    Private Sub ApplySearch()
        If AllFoodsData Is Nothing OrElse AllFoodsData.Rows.Count = 0 Then
            Return
        End If

        Dim searchText As String = txtSearch.Text.Trim()
        If String.IsNullOrEmpty(searchText) Then
            DisplayFoodsData(AllFoodsData)
            cmdClearSearch.Visible = False
            lblStatus.Text = $"Found {AllFoodsData.Rows.Count} foods in the database."
            Return
        End If

        cmdClearSearch.Visible = True
        Me.Cursor = Cursors.WaitCursor

        Try
            Dim searchField As String = If(cboSearchType.SelectedIndex = 0, "name", "barcode")
            Dim filterExpression As String

            If searchField = "name" Then
                filterExpression = $"name LIKE '%{searchText}%'"
            Else
                filterExpression = $"barcode LIKE '%{searchText}%'"
            End If

            FilteredData = New DataView(AllFoodsData)
            FilteredData.RowFilter = filterExpression
            FilteredData.Sort = "name ASC"
            dataGridFoods.DataSource = FilteredData

            lblStatus.Text = $"Found {FilteredData.Count} foods matching '{searchText}'."
            lblStatus.ForeColor = Color.White

        Catch ex As Exception
            Debug.WriteLine("Error applying search: " & ex.Message)
            lblStatus.Text = "Error applying search filter."
            lblStatus.ForeColor = ErrorColor
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub cmdSearch_Click(sender As Object, e As EventArgs)
        ApplySearch()
    End Sub

    Private Sub cmdClearSearch_Click(sender As Object, e As EventArgs)
        txtSearch.Clear()
        DisplayFoodsData(AllFoodsData)
        cmdClearSearch.Visible = False
        lblStatus.Text = $"Found {AllFoodsData.Rows.Count} foods in the database."
        lblStatus.ForeColor = Color.White
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs)
        ' Auto search if text is empty or when 3+ characters are entered
        If txtSearch.Text.Length = 0 OrElse txtSearch.Text.Length >= 3 Then
            ApplySearch()
        End If
    End Sub

    Private Sub cmdBack_Click(sender As Object, e As EventArgs)
        ' Return to the main form
        Me.Hide()
        AdminDashboard.Show()
    End Sub

    Private Sub cmdAddFood_Click(sender As Object, e As EventArgs)

        ShowFoodEditor(0) ' 0 indicates a new food
    End Sub

    Private Sub cmdEditFood_Click(sender As Object, e As EventArgs)
        ' Check if a row is selected
        If dataGridFoods.SelectedRows.Count = 0 Then
            MessageBox.Show("Please select a food to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Get the selected food ID
        Dim selectedRow As DataGridViewRow = dataGridFoods.SelectedRows(0)
        Dim foodId As Integer = Convert.ToInt32(selectedRow.Cells("food_id").Value)

        ShowFoodEditor(foodId)
    End Sub

    Private Sub ShowFoodEditor(foodId As Integer)
        Dim foodEditor As New FoodEditor(foodId)
        If foodEditor.ShowDialog() = DialogResult.OK Then
            ' Reload the food database to reflect changes
            LoadAllFoodsFromDatabase()
        End If
    End Sub

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
End Class