using System;
using System.IO;

namespace AspNetCoreRequestTracing
{
    /// <summary>
    /// <see cref="Stream"/> implementation that replicates all inputs to 2 underlying streams.
    /// </summary>
    internal class DuplicateStream : Stream
    {
        private readonly Stream _primaryStream;
        private readonly Stream _secondaryStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateStream"/> class.
        /// </summary>
        /// <param name="primaryStream">The primary <see cref="Stream"/> that is used for all results.</param>
        /// <param name="secondaryStream">The secondary <see cref="Stream"/> that will get the same input as the <paramref name="primaryStream"/>.</param>
        public DuplicateStream(
            Stream primaryStream,
            Stream secondaryStream)
        {
            _primaryStream = primaryStream ?? throw new ArgumentNullException(nameof(primaryStream));
            _secondaryStream = secondaryStream ?? throw new ArgumentNullException(nameof(secondaryStream));
        }

        /// <inheritdoc />
        public override bool CanRead => _primaryStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _primaryStream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => _primaryStream.CanWrite;

        /// <inheritdoc />
        public override long Length => _primaryStream.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => _primaryStream.Position;
            set
            {
                _primaryStream.Position = value;
                _secondaryStream.Position = value;
            }
        }

        /// <inheritdoc />
        public override void Flush()
        {
            _primaryStream.Flush();
            _secondaryStream.Flush();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = _primaryStream.Read(buffer, offset, count);
            _secondaryStream.Read(buffer, offset, count);
            return result;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            var result = _primaryStream.Seek(offset, origin);
            _secondaryStream.Seek(offset, origin);
            return result;
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            _primaryStream.SetLength(value);
            _secondaryStream.SetLength(value);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            _primaryStream.Write(buffer, offset, count);
            _secondaryStream.Write(buffer, offset, count);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _primaryStream.Dispose();
                _secondaryStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
