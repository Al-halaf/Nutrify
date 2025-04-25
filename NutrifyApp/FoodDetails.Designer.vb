<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FoodDetails
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
        Me.CloseButton = New System.Windows.Forms.Button()
        Me.txtFoodName = New System.Windows.Forms.TextBox()
        Me.txtQuantity = New System.Windows.Forms.TextBox()
        Me.txtCalories = New System.Windows.Forms.TextBox()
        Me.txtFat = New System.Windows.Forms.TextBox()
        Me.txtCarbs = New System.Windows.Forms.TextBox()
        Me.txtProtein = New System.Windows.Forms.TextBox()
        Me.txtFiber = New System.Windows.Forms.TextBox()
        Me.txtSugar = New System.Windows.Forms.TextBox()
        Me.txtSodium = New System.Windows.Forms.TextBox()
        Me.txtSaturatedFat = New System.Windows.Forms.TextBox()
        Me.txtCholesterol = New System.Windows.Forms.TextBox()
        Me.txtPotassium = New System.Windows.Forms.TextBox()
        Me.txtCalcium = New System.Windows.Forms.TextBox()
        Me.QuantityTxt = New System.Windows.Forms.TextBox()
        Me.ProteinTxt = New System.Windows.Forms.TextBox()
        Me.CaloriesTxt = New System.Windows.Forms.TextBox()
        Me.FatTxt = New System.Windows.Forms.TextBox()
        Me.FiberTxt = New System.Windows.Forms.TextBox()
        Me.Cholesterol = New System.Windows.Forms.TextBox()
        Me.CarbsTxt = New System.Windows.Forms.TextBox()
        Me.PotassiumTxt = New System.Windows.Forms.TextBox()
        Me.CalcuimTxt = New System.Windows.Forms.TextBox()
        Me.SugarTxt = New System.Windows.Forms.TextBox()
        Me.SodiumTxt = New System.Windows.Forms.TextBox()
        Me.SaturatedFatTxt = New System.Windows.Forms.TextBox()
        Me.lblServings = New System.Windows.Forms.Label()
        Me.numServings = New System.Windows.Forms.NumericUpDown()
        CType(Me.numServings, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'CloseButton
        '
        Me.CloseButton.BackColor = System.Drawing.Color.DarkRed
        Me.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.CloseButton.ForeColor = System.Drawing.Color.White
        Me.CloseButton.Location = New System.Drawing.Point(713, 12)
        Me.CloseButton.Name = "CloseButton"
        Me.CloseButton.Size = New System.Drawing.Size(109, 44)
        Me.CloseButton.TabIndex = 1
        Me.CloseButton.Text = "Close"
        Me.CloseButton.UseVisualStyleBackColor = False
        '
        'txtFoodName
        '
        Me.txtFoodName.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.txtFoodName.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtFoodName.Font = New System.Drawing.Font("Segoe UI", 20.0!, System.Drawing.FontStyle.Bold)
        Me.txtFoodName.ForeColor = System.Drawing.Color.LimeGreen
        Me.txtFoodName.Location = New System.Drawing.Point(259, 12)
        Me.txtFoodName.Name = "txtFoodName"
        Me.txtFoodName.Size = New System.Drawing.Size(329, 45)
        Me.txtFoodName.TabIndex = 2
        Me.txtFoodName.Text = "Name"
        Me.txtFoodName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtQuantity
        '
        Me.txtQuantity.ForeColor = System.Drawing.Color.Black
        Me.txtQuantity.Location = New System.Drawing.Point(259, 104)
        Me.txtQuantity.Name = "txtQuantity"
        Me.txtQuantity.Size = New System.Drawing.Size(143, 34)
        Me.txtQuantity.TabIndex = 3
        '
        'txtCalories
        '
        Me.txtCalories.ForeColor = System.Drawing.Color.Black
        Me.txtCalories.Location = New System.Drawing.Point(259, 136)
        Me.txtCalories.Name = "txtCalories"
        Me.txtCalories.Size = New System.Drawing.Size(143, 34)
        Me.txtCalories.TabIndex = 4
        '
        'txtFat
        '
        Me.txtFat.ForeColor = System.Drawing.Color.Black
        Me.txtFat.Location = New System.Drawing.Point(259, 200)
        Me.txtFat.Name = "txtFat"
        Me.txtFat.Size = New System.Drawing.Size(143, 34)
        Me.txtFat.TabIndex = 5
        '
        'txtCarbs
        '
        Me.txtCarbs.ForeColor = System.Drawing.Color.Black
        Me.txtCarbs.Location = New System.Drawing.Point(628, 105)
        Me.txtCarbs.Name = "txtCarbs"
        Me.txtCarbs.Size = New System.Drawing.Size(147, 34)
        Me.txtCarbs.TabIndex = 6
        '
        'txtProtein
        '
        Me.txtProtein.ForeColor = System.Drawing.Color.Black
        Me.txtProtein.Location = New System.Drawing.Point(259, 168)
        Me.txtProtein.Name = "txtProtein"
        Me.txtProtein.Size = New System.Drawing.Size(143, 34)
        Me.txtProtein.TabIndex = 7
        '
        'txtFiber
        '
        Me.txtFiber.ForeColor = System.Drawing.Color.Black
        Me.txtFiber.Location = New System.Drawing.Point(259, 232)
        Me.txtFiber.Name = "txtFiber"
        Me.txtFiber.Size = New System.Drawing.Size(143, 34)
        Me.txtFiber.TabIndex = 8
        '
        'txtSugar
        '
        Me.txtSugar.ForeColor = System.Drawing.Color.Black
        Me.txtSugar.Location = New System.Drawing.Point(628, 203)
        Me.txtSugar.Name = "txtSugar"
        Me.txtSugar.Size = New System.Drawing.Size(147, 34)
        Me.txtSugar.TabIndex = 9
        '
        'txtSodium
        '
        Me.txtSodium.ForeColor = System.Drawing.Color.Black
        Me.txtSodium.Location = New System.Drawing.Point(628, 235)
        Me.txtSodium.Name = "txtSodium"
        Me.txtSodium.Size = New System.Drawing.Size(147, 34)
        Me.txtSodium.TabIndex = 10
        '
        'txtSaturatedFat
        '
        Me.txtSaturatedFat.ForeColor = System.Drawing.Color.Black
        Me.txtSaturatedFat.Location = New System.Drawing.Point(628, 267)
        Me.txtSaturatedFat.Name = "txtSaturatedFat"
        Me.txtSaturatedFat.Size = New System.Drawing.Size(147, 34)
        Me.txtSaturatedFat.TabIndex = 11
        '
        'txtCholesterol
        '
        Me.txtCholesterol.ForeColor = System.Drawing.Color.Black
        Me.txtCholesterol.Location = New System.Drawing.Point(259, 264)
        Me.txtCholesterol.Name = "txtCholesterol"
        Me.txtCholesterol.Size = New System.Drawing.Size(143, 34)
        Me.txtCholesterol.TabIndex = 12
        '
        'txtPotassium
        '
        Me.txtPotassium.ForeColor = System.Drawing.Color.Black
        Me.txtPotassium.Location = New System.Drawing.Point(628, 137)
        Me.txtPotassium.Name = "txtPotassium"
        Me.txtPotassium.Size = New System.Drawing.Size(147, 34)
        Me.txtPotassium.TabIndex = 13
        '
        'txtCalcium
        '
        Me.txtCalcium.ForeColor = System.Drawing.Color.Black
        Me.txtCalcium.Location = New System.Drawing.Point(628, 169)
        Me.txtCalcium.Name = "txtCalcium"
        Me.txtCalcium.Size = New System.Drawing.Size(147, 34)
        Me.txtCalcium.TabIndex = 15
        '
        'QuantityTxt
        '
        Me.QuantityTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.QuantityTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.QuantityTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.QuantityTxt.Location = New System.Drawing.Point(94, 104)
        Me.QuantityTxt.Name = "QuantityTxt"
        Me.QuantityTxt.Size = New System.Drawing.Size(137, 27)
        Me.QuantityTxt.TabIndex = 16
        Me.QuantityTxt.Text = "Quantity: "
        '
        'ProteinTxt
        '
        Me.ProteinTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.ProteinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.ProteinTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.ProteinTxt.Location = New System.Drawing.Point(94, 168)
        Me.ProteinTxt.Name = "ProteinTxt"
        Me.ProteinTxt.Size = New System.Drawing.Size(137, 27)
        Me.ProteinTxt.TabIndex = 17
        Me.ProteinTxt.Text = "Protein: "
        '
        'CaloriesTxt
        '
        Me.CaloriesTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.CaloriesTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.CaloriesTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.CaloriesTxt.Location = New System.Drawing.Point(94, 136)
        Me.CaloriesTxt.Name = "CaloriesTxt"
        Me.CaloriesTxt.Size = New System.Drawing.Size(137, 27)
        Me.CaloriesTxt.TabIndex = 18
        Me.CaloriesTxt.Text = "Calories: "
        '
        'FatTxt
        '
        Me.FatTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.FatTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.FatTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.FatTxt.Location = New System.Drawing.Point(94, 200)
        Me.FatTxt.Name = "FatTxt"
        Me.FatTxt.Size = New System.Drawing.Size(137, 27)
        Me.FatTxt.TabIndex = 19
        Me.FatTxt.Text = "Fat: "
        '
        'FiberTxt
        '
        Me.FiberTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.FiberTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.FiberTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.FiberTxt.Location = New System.Drawing.Point(94, 232)
        Me.FiberTxt.Name = "FiberTxt"
        Me.FiberTxt.Size = New System.Drawing.Size(137, 27)
        Me.FiberTxt.TabIndex = 20
        Me.FiberTxt.Text = "Fiber: "
        '
        'Cholesterol
        '
        Me.Cholesterol.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.Cholesterol.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.Cholesterol.ForeColor = System.Drawing.Color.LimeGreen
        Me.Cholesterol.Location = New System.Drawing.Point(94, 264)
        Me.Cholesterol.Name = "Cholesterol"
        Me.Cholesterol.Size = New System.Drawing.Size(137, 27)
        Me.Cholesterol.TabIndex = 21
        Me.Cholesterol.Text = "Cholesterol: "
        '
        'CarbsTxt
        '
        Me.CarbsTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.CarbsTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.CarbsTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.CarbsTxt.Location = New System.Drawing.Point(464, 105)
        Me.CarbsTxt.Name = "CarbsTxt"
        Me.CarbsTxt.Size = New System.Drawing.Size(135, 27)
        Me.CarbsTxt.TabIndex = 22
        Me.CarbsTxt.Text = "Carbs: "
        '
        'PotassiumTxt
        '
        Me.PotassiumTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.PotassiumTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.PotassiumTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.PotassiumTxt.Location = New System.Drawing.Point(464, 137)
        Me.PotassiumTxt.Name = "PotassiumTxt"
        Me.PotassiumTxt.Size = New System.Drawing.Size(135, 27)
        Me.PotassiumTxt.TabIndex = 23
        Me.PotassiumTxt.Text = "Potassium: "
        '
        'CalcuimTxt
        '
        Me.CalcuimTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.CalcuimTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.CalcuimTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.CalcuimTxt.Location = New System.Drawing.Point(464, 169)
        Me.CalcuimTxt.Name = "CalcuimTxt"
        Me.CalcuimTxt.Size = New System.Drawing.Size(135, 27)
        Me.CalcuimTxt.TabIndex = 24
        Me.CalcuimTxt.Text = "Calcium: "
        '
        'SugarTxt
        '
        Me.SugarTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.SugarTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.SugarTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.SugarTxt.Location = New System.Drawing.Point(464, 201)
        Me.SugarTxt.Name = "SugarTxt"
        Me.SugarTxt.Size = New System.Drawing.Size(135, 27)
        Me.SugarTxt.TabIndex = 25
        Me.SugarTxt.Text = "Sugar: "
        '
        'SodiumTxt
        '
        Me.SodiumTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.SodiumTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.SodiumTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.SodiumTxt.Location = New System.Drawing.Point(464, 233)
        Me.SodiumTxt.Name = "SodiumTxt"
        Me.SodiumTxt.Size = New System.Drawing.Size(135, 27)
        Me.SodiumTxt.TabIndex = 26
        Me.SodiumTxt.Text = "Sodium: "
        '
        'SaturatedFatTxt
        '
        Me.SaturatedFatTxt.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.SaturatedFatTxt.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.SaturatedFatTxt.ForeColor = System.Drawing.Color.LimeGreen
        Me.SaturatedFatTxt.Location = New System.Drawing.Point(464, 265)
        Me.SaturatedFatTxt.Name = "SaturatedFatTxt"
        Me.SaturatedFatTxt.Size = New System.Drawing.Size(135, 27)
        Me.SaturatedFatTxt.TabIndex = 27
        Me.SaturatedFatTxt.Text = "SaturatedFat: "
        '
        'lblServings
        '
        Me.lblServings.AutoSize = True
        Me.lblServings.ForeColor = System.Drawing.Color.LimeGreen
        Me.lblServings.Location = New System.Drawing.Point(337, 334)
        Me.lblServings.Name = "lblServings"
        Me.lblServings.Size = New System.Drawing.Size(104, 28)
        Me.lblServings.TabIndex = 28
        Me.lblServings.Text = "Servings: "
        '
        'numServings
        '
        Me.numServings.Location = New System.Drawing.Point(459, 332)
        Me.numServings.Name = "numServings"
        Me.numServings.Size = New System.Drawing.Size(66, 34)
        Me.numServings.TabIndex = 29
        '
        'FoodDetails
        '
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(834, 397)
        Me.Controls.Add(Me.numServings)
        Me.Controls.Add(Me.lblServings)
        Me.Controls.Add(Me.SaturatedFatTxt)
        Me.Controls.Add(Me.SodiumTxt)
        Me.Controls.Add(Me.SugarTxt)
        Me.Controls.Add(Me.CalcuimTxt)
        Me.Controls.Add(Me.PotassiumTxt)
        Me.Controls.Add(Me.CarbsTxt)
        Me.Controls.Add(Me.Cholesterol)
        Me.Controls.Add(Me.FiberTxt)
        Me.Controls.Add(Me.FatTxt)
        Me.Controls.Add(Me.CaloriesTxt)
        Me.Controls.Add(Me.ProteinTxt)
        Me.Controls.Add(Me.QuantityTxt)
        Me.Controls.Add(Me.txtCalcium)
        Me.Controls.Add(Me.txtPotassium)
        Me.Controls.Add(Me.txtCholesterol)
        Me.Controls.Add(Me.txtSaturatedFat)
        Me.Controls.Add(Me.txtSodium)
        Me.Controls.Add(Me.txtSugar)
        Me.Controls.Add(Me.txtFiber)
        Me.Controls.Add(Me.txtProtein)
        Me.Controls.Add(Me.txtCarbs)
        Me.Controls.Add(Me.txtFat)
        Me.Controls.Add(Me.txtCalories)
        Me.Controls.Add(Me.txtQuantity)
        Me.Controls.Add(Me.txtFoodName)
        Me.Controls.Add(Me.CloseButton)
        Me.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Name = "FoodDetails"
        CType(Me.numServings, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents CloseButton As Button
    Friend WithEvents txtFoodName As TextBox
    Friend WithEvents txtQuantity As TextBox
    Friend WithEvents txtCalories As TextBox
    Friend WithEvents txtFat As TextBox
    Friend WithEvents txtCarbs As TextBox
    Friend WithEvents txtProtein As TextBox
    Friend WithEvents txtFiber As TextBox
    Friend WithEvents txtSugar As TextBox
    Friend WithEvents txtSodium As TextBox
    Friend WithEvents txtSaturatedFat As TextBox
    Friend WithEvents txtCholesterol As TextBox
    Friend WithEvents txtPotassium As TextBox
    Friend WithEvents txtCalcium As TextBox
    Friend WithEvents QuantityTxt As TextBox
    Friend WithEvents ProteinTxt As TextBox
    Friend WithEvents CaloriesTxt As TextBox
    Friend WithEvents FatTxt As TextBox
    Friend WithEvents FiberTxt As TextBox
    Friend WithEvents Cholesterol As TextBox
    Friend WithEvents CarbsTxt As TextBox
    Friend WithEvents PotassiumTxt As TextBox
    Friend WithEvents CalcuimTxt As TextBox
    Friend WithEvents SugarTxt As TextBox
    Friend WithEvents SodiumTxt As TextBox
    Friend WithEvents SaturatedFatTxt As TextBox
    Friend WithEvents lblServings As Label
    Friend WithEvents numServings As NumericUpDown
End Class
