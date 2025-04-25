<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ManageMeals
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.BackButton = New System.Windows.Forms.Button()
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.MealsTxt = New System.Windows.Forms.TextBox()
        Me.SearchTextBox = New System.Windows.Forms.TextBox()
        Me.ClearSearchButton = New System.Windows.Forms.Button()
        Me.DateFilterPicker = New System.Windows.Forms.DateTimePicker()
        Me.DateFilterLabel = New System.Windows.Forms.Label()
        Me.DateFilterComboBox = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.barcodeButton = New System.Windows.Forms.Button()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'BackButton
        '
        Me.BackButton.BackColor = System.Drawing.Color.DarkRed
        Me.BackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BackButton.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BackButton.ForeColor = System.Drawing.Color.White
        Me.BackButton.Location = New System.Drawing.Point(779, 11)
        Me.BackButton.Name = "BackButton"
        Me.BackButton.Size = New System.Drawing.Size(89, 44)
        Me.BackButton.TabIndex = 0
        Me.BackButton.Text = "← Back"
        Me.BackButton.UseVisualStyleBackColor = False
        '
        'DataGridView1
        '
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Location = New System.Drawing.Point(94, 149)
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.RowHeadersWidth = 62
        Me.DataGridView1.Size = New System.Drawing.Size(674, 296)
        Me.DataGridView1.TabIndex = 1
        '
        'MealsTxt
        '
        Me.MealsTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.MealsTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.MealsTxt.Enabled = False
        Me.MealsTxt.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.MealsTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.MealsTxt.Location = New System.Drawing.Point(94, 87)
        Me.MealsTxt.Name = "MealsTxt"
        Me.MealsTxt.ReadOnly = True
        Me.MealsTxt.Size = New System.Drawing.Size(111, 22)
        Me.MealsTxt.TabIndex = 4
        Me.MealsTxt.Text = "Meals Logged"
        '
        'SearchTextBox
        '
        Me.SearchTextBox.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.SearchTextBox.Location = New System.Drawing.Point(286, 87)
        Me.SearchTextBox.Name = "SearchTextBox"
        Me.SearchTextBox.Size = New System.Drawing.Size(130, 29)
        Me.SearchTextBox.TabIndex = 6
        '
        'ClearSearchButton
        '
        Me.ClearSearchButton.BackColor = System.Drawing.Color.LimeGreen
        Me.ClearSearchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ClearSearchButton.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ClearSearchButton.ForeColor = System.Drawing.Color.Black
        Me.ClearSearchButton.Location = New System.Drawing.Point(433, 85)
        Me.ClearSearchButton.Name = "ClearSearchButton"
        Me.ClearSearchButton.Size = New System.Drawing.Size(69, 31)
        Me.ClearSearchButton.TabIndex = 7
        Me.ClearSearchButton.Text = "Clear"
        Me.ClearSearchButton.UseVisualStyleBackColor = False
        '
        'DateFilterPicker
        '
        Me.DateFilterPicker.CalendarFont = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.DateFilterPicker.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.DateFilterPicker.Location = New System.Drawing.Point(552, 62)
        Me.DateFilterPicker.Name = "DateFilterPicker"
        Me.DateFilterPicker.Size = New System.Drawing.Size(163, 29)
        Me.DateFilterPicker.TabIndex = 8
        '
        'DateFilterLabel
        '
        Me.DateFilterLabel.AutoSize = True
        Me.DateFilterLabel.Location = New System.Drawing.Point(603, 26)
        Me.DateFilterLabel.Name = "DateFilterLabel"
        Me.DateFilterLabel.Size = New System.Drawing.Size(49, 13)
        Me.DateFilterLabel.TabIndex = 9
        Me.DateFilterLabel.Text = "Filter by: "
        '
        'DateFilterComboBox
        '
        Me.DateFilterComboBox.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.DateFilterComboBox.ForeColor = System.Drawing.Color.Black
        Me.DateFilterComboBox.FormattingEnabled = True
        Me.DateFilterComboBox.Location = New System.Drawing.Point(552, 26)
        Me.DateFilterComboBox.Name = "DateFilterComboBox"
        Me.DateFilterComboBox.Size = New System.Drawing.Size(163, 29)
        Me.DateFilterComboBox.TabIndex = 10
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 22.2!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.LimeGreen
        Me.Label1.Location = New System.Drawing.Point(87, 26)
        Me.Label1.Margin = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(150, 41)
        Me.Label1.TabIndex = 11
        Me.Label1.Text = "Nutrition"
        '
        'barcodeButton
        '
        Me.barcodeButton.BackColor = System.Drawing.Color.LimeGreen
        Me.barcodeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.barcodeButton.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.barcodeButton.ForeColor = System.Drawing.Color.Black
        Me.barcodeButton.Location = New System.Drawing.Point(363, 467)
        Me.barcodeButton.Name = "barcodeButton"
        Me.barcodeButton.Size = New System.Drawing.Size(153, 31)
        Me.barcodeButton.TabIndex = 12
        Me.barcodeButton.Text = "Scan barcode"
        Me.barcodeButton.UseVisualStyleBackColor = False
        '
        'ManageMeals
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(880, 510)
        Me.Controls.Add(Me.barcodeButton)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.DateFilterComboBox)
        Me.Controls.Add(Me.DateFilterLabel)
        Me.Controls.Add(Me.DateFilterPicker)
        Me.Controls.Add(Me.ClearSearchButton)
        Me.Controls.Add(Me.SearchTextBox)
        Me.Controls.Add(Me.MealsTxt)
        Me.Controls.Add(Me.DataGridView1)
        Me.Controls.Add(Me.BackButton)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "ManageMeals"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Nutrition"
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents BackButton As Button
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents MealsTxt As TextBox
    Friend WithEvents SearchTextBox As TextBox
    Friend WithEvents ClearSearchButton As Button
    Friend WithEvents DateFilterPicker As DateTimePicker
    Friend WithEvents DateFilterLabel As Label
    Friend WithEvents DateFilterComboBox As ComboBox
    Friend WithEvents Label1 As Label
    Friend WithEvents barcodeButton As Button
End Class
