using System;
using System.Threading;
using System.Windows.Forms;

namespace Gw2Launcher.Util
{
    static class Invoke
    {
        /// <summary>
        /// Executes the action on the control's thread, but only if invoking was required
        /// </summary>
        /// <returns>True if the action was executed</returns>
        public static bool IfRequired(Control c, Action a)
        {
            if (c == null || c.InvokeRequired)
            {
                try
                {
                    c.Invoke(a);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Executes the action on the control's thread
        /// </summary>
        public static void Required(Control c, Action a)
        {
            if (c == null || c.InvokeRequired)
            {
                try
                {
                    c.Invoke(a);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
            else
            {
                a();
            }
        }

        /// <summary>
        /// Executes the action asynchronously on the control's thread, but only if invoking was required
        /// </summary>
        /// <returns>True if the action was executed</returns>
        public static bool IfRequiredAsync(Control c, Action a)
        {
            if (c == null || c.InvokeRequired)
            {
                try
                {
                    c.BeginInvoke(a);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Executes the action asynchronously on the control's thread
        /// </summary>
        public static void Async(Control c, Action a)
        {
            try
            {
                c.BeginInvoke(a);
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }
    }
}
