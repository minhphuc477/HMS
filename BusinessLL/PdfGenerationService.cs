using BarcodeLib;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System;
using BarcodeLib.Barcode;
using System.Drawing;
using System.IO;
using iText.Layout.Properties;
using iText.IO.Image;
using BarcodeStandard;
using DataTransferO; // Add this directive

namespace BusinessLL
{
    public class PdfGenerationService
    {
        public void GenerateAppointmentPdf(AppointmentDTO appointment, Stream outputStream, UserDTO user)
        {
            if (appointment == null) throw new ArgumentNullException(nameof(appointment));
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));
            if (user == null) throw new ArgumentNullException(nameof(user));

            using (var writer = new PdfWriter(new NonClosingStream(outputStream))) // Wrap the output stream
            {
                var pdfDocument = new PdfDocument(writer);
                var document = new Document(pdfDocument);

                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var title = new Paragraph("Appointment Details")
                    .SetFont(font)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER);

                // Draw barcode
                System.Drawing.Image barcodeImage = GenerateBarcode(appointment.AppointmentId.ToString());
                var barcodeImageElement = CreateBarcodeImageElement(barcodeImage);
                barcodeImageElement.SetWidth(500); // Adjust size
                barcodeImageElement.SetHeight(50); // Adjust size

                // Create a table to organize the content
                var table = new Table(2);
                table.SetWidth(UnitValue.CreatePercentValue(100));

                // Add barcode and title to the table
                table.AddCell(new Cell(1, 2).Add(barcodeImageElement).SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell(1, 2).Add(title).SetTextAlignment(TextAlignment.CENTER));


                table.AddCell(new Cell().Add(new Paragraph($"Name: {user.Name ?? $"UserId: {appointment.PatientId}"}").SetFont(font).SetFontSize(12)));
                table.AddCell(new Cell().Add(new Paragraph($"Email: {user.Email ?? "N/A"}").SetFont(font).SetFontSize(12)));

                // Add doctor information
                table.AddCell(new Cell().Add(new Paragraph($"Doctor: {appointment.Doctor?.User?.Name ?? "N/A"}").SetFont(font).SetFontSize(12)));
                table.AddCell(new Cell().Add(new Paragraph($"Department: {appointment.Department?.DepartmentName ?? "N/A"}").SetFont(font).SetFontSize(12)));

                // Add appointment date and notes
                table.AddCell(new Cell().Add(new Paragraph($"Date: {appointment.FormattedAppointmentDate}").SetFont(font).SetFontSize(12)));
                table.AddCell(new Cell().Add(new Paragraph($"Notes: {appointment.Notes ?? "None"}").SetFont(font).SetFontSize(12)));

                document.Add(table);
                document.Close();
            }
        }

        private System.Drawing.Image GenerateBarcode(string appointmentId)
        {
            if (string.IsNullOrWhiteSpace(appointmentId)) throw new ArgumentNullException(nameof(appointmentId));

            BarcodeLib.Barcode.Linear barcode = new BarcodeLib.Barcode.Linear
            {
                Type = BarcodeLib.Barcode.BarcodeType.CODE128,
                ProcessTilde = true,
                Data = $"{appointmentId}~013" // Append carriage return character
            };

            // Generate the barcode as an Image
            return barcode.drawBarcode();
        }

        private iText.Layout.Element.Image CreateBarcodeImageElement(System.Drawing.Image barcodeImage)
        {
            if (barcodeImage == null) throw new ArgumentNullException(nameof(barcodeImage));

            using (MemoryStream ms = new MemoryStream())
            {
                barcodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                var imageData = ImageDataFactory.Create(ms.ToArray());
                var image = new iText.Layout.Element.Image(imageData);
                return image;
            }
        }

        // Add the NonClosingStream class here
        public class NonClosingStream : Stream
        {
            private readonly Stream _innerStream;

            public NonClosingStream(Stream innerStream)
            {
                _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
            }

            public override bool CanRead => _innerStream.CanRead;
            public override bool CanSeek => _innerStream.CanSeek;
            public override bool CanWrite => _innerStream.CanWrite;
            public override long Length => _innerStream.Length;

            public override long Position
            {
                get => _innerStream.Position;
                set => _innerStream.Position = value;
            }

            public override void Flush() => _innerStream.Flush();

            public override int Read(byte[] buffer, int offset, int count) =>
                _innerStream.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) =>
                _innerStream.Seek(offset, origin);

            public override void SetLength(long value) => _innerStream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) =>
                _innerStream.Write(buffer, offset, count);

            // Prevent the inner stream from being closed
            protected override void Dispose(bool disposing)
            {
                // Only flush if the stream is still writable
                if (disposing && _innerStream.CanWrite)
                {
                    _innerStream.Flush();
                }

                // Do not close the inner stream
            }
        }
    }
}
