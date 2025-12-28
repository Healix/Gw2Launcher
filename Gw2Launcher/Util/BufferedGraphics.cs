using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Gw2Launcher.Util
{
    class BufferedGraphics : IDisposable
    {
        [Flags]
        public enum RedrawType : byte
        {
            None = 0,
            Full = 1,
            Partial = 2,
        }

        private System.Drawing.BufferedGraphics buffer;
        private Rectangle clip, bounds;
        private RedrawType redraw;

        public BufferedGraphics()
        {
        }

        ~BufferedGraphics()
        {
            Dispose();
        }

        public void Allocate(Graphics g, Rectangle bounds)
        {
            if (this.buffer != null)
            {
                this.buffer.Dispose();
            }
            this.bounds = bounds;
            this.buffer = System.Drawing.BufferedGraphicsManager.Current.Allocate(g, bounds);
        }

        public bool Allocated
        {
            get
            {
                return this.buffer != null;
            }
        }

        public Graphics Graphics
        {
            get
            {
                return this.buffer.Graphics;
            }
        }

        /// <summary>
        /// Target rectangle for drawing
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public void Deallocate()
        {
            if (this.buffer != null)
            {
                this.buffer.Dispose();
                this.buffer = null;
            }
        }

        public void Render(Graphics g)
        {
            if (this.buffer != null)
                this.buffer.Render(g);
        }

        /// <summary>
        /// Pending redraw request
        /// </summary>
        public RedrawType Redraw
        {
            get
            {
                return redraw;
            }
        }

        /// <summary>
        /// True if there's a pending redraw request
        /// </summary>
        public bool Clipped
        {
            get
            {
                return redraw != RedrawType.Full;
            }
        }

        /// <summary>
        /// Area that has been invalidated
        /// </summary>
        public Rectangle ClipRectangle
        {
            get
            {
                if (redraw == RedrawType.Partial)
                {
                    return clip;
                }
                else
                {
                    return bounds;
                }
            }
        }

        /// <summary>
        /// Resets redraw
        /// </summary>
        public void ResetClip()
        {
            redraw = RedrawType.None;
        }

        public bool Invalidate(System.Windows.Forms.Control source, Rectangle r)
        {
            if (redraw != RedrawType.Full)
            {
                if (redraw == RedrawType.None)
                {
                    clip = r;
                    redraw = RedrawType.Partial;
                }
                else if (clip.Contains(r))
                {
                    return false;
                }
                else
                {
                    clip = Rectangle.Union(clip, r);
                }

                source.Invalidate(r);

                return true;
            }

            return false;
        }

        public bool Invalidate(System.Windows.Forms.Control source)
        {
            if (redraw != RedrawType.Full)
            {
                redraw = RedrawType.Full;
                clip = Rectangle.Empty;

                source.Invalidate();

                return true;
            }

            return false;
        }

        public void Dispose()
        {
            Deallocate();
        }
    }
}
