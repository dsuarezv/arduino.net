/************************************************************************

   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the New BSD
   License (BSD) as published at http://avalondock.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up AvalonDock in Extended WPF Toolkit Plus at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like facebook.com/datagrids

  **********************************************************************/

using System;
using System.Windows;
using System.Windows.Threading;



namespace AvalonDock
{

    /// <summary>
    /// Designates a Windows Presentation Foundation application model with added functionalities.
    /// </summary>
    class WpfApplication : Application
    {

        private static DispatcherOperationCallback exitFrameCallback = new
                                DispatcherOperationCallback(ExitFrame);



        /// <summary>
        /// Processes all UI messages currently in the message queue.
        /// </summary>
        public static void DoEvents()
        {

            // Create new nested message pump.
            DispatcherFrame nestedFrame = new DispatcherFrame();



            // Dispatch a callback to the current message queue, when getting called, 
            // this callback will end the nested message loop.
            // note that the priority of this callback should be lower than the that of UI event messages.
            DispatcherOperation exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(
                                                  DispatcherPriority.Background, exitFrameCallback, nestedFrame);



            // pump the nested message loop, the nested message loop will 
            // immediately process the messages left inside the message queue.
            Dispatcher.PushFrame(nestedFrame);



            // If the "exitFrame" callback doesn't get finished, Abort it.
            if (exitOperation.Status != DispatcherOperationStatus.Completed)
            {
                exitOperation.Abort();
            }

        }



        private static Object ExitFrame(Object state)
        {
            DispatcherFrame frame = state as DispatcherFrame;


            // Exit the nested message loop.
            frame.Continue = false;

            return null;

        }

    }

}

