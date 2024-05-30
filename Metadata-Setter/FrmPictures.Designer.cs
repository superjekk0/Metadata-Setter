namespace Metadata_Setter
{
    partial class FrmPictures
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblDescription = new Label();
            lsvPictures = new ListView();
            btnAccept = new Button();
            btnReset = new Button();
            btnRemove = new Button();
            btnAdd = new Button();
            SuspendLayout();
            // 
            // lblDescription
            // 
            lblDescription.Font = new Font("Segoe UI Semibold", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDescription.Location = new Point(23, 18);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(336, 48);
            lblDescription.TabIndex = 0;
            lblDescription.Text = "Select images to link as metadata";
            lblDescription.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lsvPictures
            // 
            lsvPictures.Location = new Point(12, 105);
            lsvPictures.Name = "lsvPictures";
            lsvPictures.Size = new Size(360, 213);
            lsvPictures.TabIndex = 1;
            lsvPictures.UseCompatibleStateImageBehavior = false;
            // 
            // btnAccept
            // 
            btnAccept.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnAccept.Location = new Point(23, 324);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(336, 34);
            btnAccept.TabIndex = 2;
            btnAccept.Text = "Select those pictures";
            btnAccept.UseVisualStyleBackColor = true;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(23, 69);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(102, 30);
            btnReset.TabIndex = 3;
            btnReset.Text = "Reset collection";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += BtnReset_Click;
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(140, 70);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(108, 29);
            btnRemove.TabIndex = 4;
            btnRemove.Text = "Remove selection";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += this.BtnRemove_Click;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(263, 72);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(96, 27);
            btnAdd.TabIndex = 5;
            btnAdd.Text = "Add pictures";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += BtnAdd_Click;
            // 
            // FrmPictures
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 361);
            Controls.Add(btnAdd);
            Controls.Add(btnRemove);
            Controls.Add(btnReset);
            Controls.Add(btnAccept);
            Controls.Add(lsvPictures);
            Controls.Add(lblDescription);
            Name = "FrmPictures";
            Text = "Add pictures";
            ResumeLayout(false);
        }

        #endregion

        private Label lblDescription;
        private ListView lsvPictures;
        private Button btnAccept;
        private Button btnReset;
        private Button btnRemove;
        private Button btnAdd;
    }
}