using BusinessLL;
using PdfiumViewer;
using System;
using System.IO;
using System.Windows.Forms;
using DataTransferO; // Add this directive

namespace HMS_CHANGE.Patient.Appointment
{
    public partial class PdfViewerForm : Form
    {
        private Button savePdfButton;
        private PdfViewer pdfViewer; // PdfiumViewer control
        private Stream? pdfStream;
        private readonly PdfGenerationService pdfGenerationService;
        private UserDTO? currentUser;

        public PdfViewerForm()
        {
            InitializeComponent();
            InitializePdfViewer();
            InitializeSaveButton();
            pdfGenerationService = new PdfGenerationService();
        }

        private void InitializePdfViewer()
        {
            pdfViewer = new PdfViewer
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(pdfViewer);
        }

        private void InitializeSaveButton()
        {
            savePdfButton = new Button
            {
                Text = "Save PDF",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            savePdfButton.Click += SavePdfButton_Click;
            this.Controls.Add(savePdfButton);
        }

        public void LoadPdf(Stream pdfStream, UserDTO user)
        {
            this.pdfStream = new PdfGenerationService.NonClosingStream(pdfStream);
            this.currentUser = user;
            try
            {
                pdfViewer.Document = PdfDocument.Load(this.pdfStream);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SavePdfButton_Click(object sender, EventArgs e)
        {
            if (pdfStream == null)
            {
                MessageBox.Show("No PDF loaded to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                saveFileDialog.Title = "Save PDF";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Reset the stream position before copying
                        if (pdfStream.CanSeek)
                        {
                            pdfStream.Position = 0;
                        }

                        using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                        {
                            pdfStream.CopyTo(fileStream);
                        }

                        MessageBox.Show("PDF saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            pdfStream?.Dispose();
        }
    }
}
