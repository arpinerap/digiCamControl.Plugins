using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Macrophotography.Classes
{
    class ZereneLaunchProof
    {
    }}
        /*
        bool mExternalProcessDone;
	    bool mExternalProcessSuccessfullyStarted;
	    Process mExternalProcess;
	    Thread mDrainOutputThread;
	    Thread mDrainErrorsThread;
	    Thread mWaitforThread;
	    string mLaunchCmdDir;
	    String mLaunchCmdFile;
	    //ProgressFrame mProgressFrame;


        static bool FileInUse(string path)
    {
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                fs.CanWrite
            }
            return false;
        }
        catch (IOException ex)
        {
            return true;
        }
    }


        string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

	    /**
	     * Launches and waits for Zerene Stacker to process the default batch script
	     * ZereneBatch.xml in a specified directory.
	     * 
	     * @param directoryPath  fully qualified pathname of the directory to be processed
	     */
        /*
	public void launchAndWaitForZereneStacker(String directoryPath) 
    {
		
		// The following definition for launchCmdDirName is appropriate for Windows 7.
		// Other versions of Windows will require different definitions.  You can determine
		// the appropriate definition for your version of Windows by opening a
		// Windows Explorer and visiting the folder named %APPDATA% .

		mLaunchCmdDir = "C:\\Users\\"+userName+"\\AppData\\Roaming"+"\\ZereneStacker";
		
		// To begin, we read the launch command left over from the previous execution
		// of Zerene Stacker as a stand-alone application by this user.
		// If the launch command has not been read, then we attempt a graceful recovery,
		// but this will probably require at least some user interaction to complete
		// the process.
		
        string fileName = "zerenstk.launchcmd";




		//mLaunchCmdFile = new File(mLaunchCmdDir,"zerenstk.launchcmd");
        mLaunchCmdFile = Path.Combine(mLaunchCmdDir, fileName);


        string arg = @" -jar jcgp.jar -user=someone";// just an example this can be anything
        string command = "java.exe";
        ProcessStartInfo start = new ProcessStartInfo(command, arg);
        start.UseShellExecute = false;
        start.CreateNoWindow = true; // Important if you want to keep shell window hidden
        Process.Start(start).WaitForExit(); //important to add WaitForExit()


		if (!File.Exists(fileName) || !FileInUse(fileName)) {
			attemptGracefulRecoveryFromNoPreviousLaunch();
		}
		string launchCommand = null;
		try {
			BufferedStream br = new BufferedStream(mLaunchCmdFile);
			launchCommand = br.readLine();
		} catch (Exception e) {
			e.printStackTrace();
			System.exit(0);
		}
		System.out.println("cmd from zerenstk.launchcmd = "+launchCommand);
		
		launchCommand = launchCommand.replace("javaw.exe","java.exe");  // to make SHOWPROGRESS lines available 
		launchCommand += " -noSplashScreen";
		launchCommand += " -exitOnBatchScriptCompletion";
		launchCommand += " -runMinimized";
		launchCommand += " ";
		launchCommand += directoryPath;
		
		System.out.println("launchCommand as executed = "+launchCommand);
		
		// Create a frame containing a progress bar of our own.  For demonstration purposes,
		// it will be updated in synchronization with Zerene Stacker's own progress bar.
		// In practice, if the calling program is going to maintain its own progress bar,
		// then Zerene Stacker should be invoked with a runtime parameter of
		// -showProgressWhenMinimized=false
		// to prevent Zerene Stacker from showing its own progress bar.
		// This parameter is only supported in Zerene Stacker at Build T201303122140 and above.
		
		mProgressFrame = new ProgressFrame();

		
		// Now launch the Zerene Stacker process and start several threads
		// to monitor its progress.
		try {
			System.out.println("Starting external process to run Zerene Stacker");
			mExternalProcess = Runtime.getRuntime().exec(launchCommand);
			System.out.println("External process started");
			
			// Start a thread to monitor for external process completed.
			mExternalProcessDone = false;
			mWaitforThread = new Thread() {
	
            public void run() {
					while (!mExternalProcessDone) {
						try {
							mExternalProcess.waitFor();
							System.out.println("Saw external process complete");
							mExternalProcessDone = true;
						} catch (InterruptedException e) {
							// ignore and retry
						}
					}
				}
			};
			System.out.println("Starting wait-for thread.");
			mWaitforThread.start();
			
			// Start a thread to catch error output from the external process
			// and turn it into error log messages for the current application.
			mDrainErrorsThread = new Thread() {
				public void run() {
					InputStream errorStream = mExternalProcess.getErrorStream();
					BufferedReader be = new BufferedReader(new InputStreamReader(errorStream));
					String s;
					try {
						while ((s = be.readLine()) != null || !mExternalProcessDone) {
							if (s != null) {
								System.out.println("From Zerene Stacker error stream: "+s);
							}
						}
					} catch (IOException e) {
						System.err.println("Received IOException on Zerene Stacker error stream.");
					}
					System.out.println("Drain-errors thread terminating.");
				}
			};
			System.out.println("Starting drain-errors thread.");
			mDrainErrorsThread.start();
			
			// Start a thread to catch normal output from the external process.
			// In this case, we're ignoring all messages except the ones that are
			// required to update the progress bar.
			mDrainOutputThread = new Thread() {
				public void run() {
					InputStream in = mExternalProcess.getInputStream();
					BufferedReader br = new BufferedReader(new InputStreamReader(in));
					String s;
					try {
						while ((s = br.readLine()) != null || !mExternalProcessDone) {
							if (s != null) {
//								System.out.println("From Zerene Stacker: "+s);
								if (s.contains("SHOWPROGRESS")) {
									mProgressFrame.notify(s);
								}
							}
						}
					} catch (IOException e) {
						System.out.println("Received IOException on Zerene Stacker output stream.");
					}
					System.out.println("Drain-output thread terminating.");
				}
			};
			System.out.println("Starting drain-output thread.");
			mDrainOutputThread.start();
			mExternalProcessSuccessfullyStarted = true;
			
		} catch (IOException e) {
			System.err.println("SampleLaunchProgram was unable to execute Zerene Stacker.");
		}
		
		// If the launch was successful, then wait for all the monitoring threads to terminate.
		if (mExternalProcessSuccessfullyStarted) {
			while (mWaitforThread.isAlive()) {
				try {
					mWaitforThread.join();
				} catch (InterruptedException e) {
				}
			}
			while (mDrainErrorsThread.isAlive()) {
				try {
					mDrainErrorsThread.join();
				} catch (InterruptedException e) {
				}
			}
			while (mDrainOutputThread.isAlive()) {
				try {
					mDrainOutputThread.join();
				} catch (InterruptedException e) {
				}
			}
			System.out.println("All threads now joined.");
		}
		
		mProgressFrame.dispose();  // get rid of the progress bar to allow program termination
		
		// All done -- Zerene Stacker has now terminated.
		return;
	}
	
	// This method attempts a graceful recovery in the event that the current user
	// has never launched and configured Zerene Stacker as a stand-alone application.
	// It works by creating the directory that was expected to be found, and writing
	// into that directory the same command line that would have been written
	// by the stand-alone launch stub.  Note that portions of this command line
	// depend on things like where Zerene Stacker was installed, which are not
	// really knowable by this SampleLaunchProgram class.  What appears here
	// are the defaults for installation of 32-bit Zerene Stacker under 32-bit
	// Windows XP, for a system capable of supporting 1600 MB of heap space.
	
	private void attemptGracefulRecoveryFromNoPreviousLaunch() 
    {
		mLaunchCmdDir.mkdirs();
		if (!mLaunchCmdDir.exists()) {
			System.err.println("Cannot create Zerene Stacker directory "+mLaunchCmdDir);
			System.exit(1);
		}
		String defaultCmd = "\"C:\\Program Files\\ZereneStacker\\jre6\\bin\\javaw.exe\"" +
							" -Xmx1600m -DjavaBits=32bitJava" +
							" -Dlaunchcmddir=" + "\"" + mLaunchCmdDirName + "\"" +
							" -classpath \"C:\\Program Files\\ZereneStacker\\ZereneStacker.jar;" + 
							"C:\\Program Files\\ZereneStacker\\JREextensions\\*\"" +
							" com.zerenesystems.stacker.gui.MainFrame";
		try {
			PrintStream ps = new PrintStream(mLaunchCmdFile);
			ps.println(defaultCmd);
			ps.close();
		} catch (IOException e1) {
			e1.printStackTrace();
		}
	}
	
	/**
	 * Stand alone program for testing.
	 * @param args  unused for this test program
	 */


        /*
	public static void main(String[] args) 
{
		String directoryPath = "D:/ZereneSystems/DemoFolder";
		(new SampleLaunchProgram()).launchAndWaitForZereneStacker(directoryPath);
	}

	/**
	 * ProgressFrame: an internal class to present a progress bar in a window owned by the calling program.
	 *
	 */



        /*
	class ProgressFrame extends JFrame {
		JProgressBar mProgressBar;		
		
		public ProgressFrame() {
			super("Zerene Stacker Progress");
			mProgressBar = new JProgressBar();
			mProgressBar.setSize(new Dimension(400,50));
			mProgressBar.setVisible(false);
			mProgressBar.setIndeterminate(true);
			getContentPane().add(mProgressBar);
			setBounds(new Rectangle(300,400,400,100));
			validate();
			setVisible(true);
		}
		
		/*
		 * Notify the ProgressFrame class of an incoming change of status
		 * @param s  any of the "SHOWPROGRESS:" lines produced by Zerene Stacker
		 */


        /*
		public void notify(final String s) {
			System.out.println("notify: "+s);
			try {
				SwingUtilities.invokeAndWait(new Runnable() {  // execute on event thread because Swing is not thread-safe
					public void run() {
						if (s.contains("SHOWPROGRESS: Done")) {
							mProgressBar.setVisible(false);
						} else if (s.contains("SHOWPROGRESS: Show")) {
							mProgressBar.setVisible(true);
						} else if (s.contains("SHOWPROGRESS: Indeterminate")) {
							mProgressBar.setIndeterminate(true);
							mProgressBar.setStringPainted(false);  // do not show progress as percent
						} else if (s.contains("SHOWPROGRESS: Max")) {
							int max = Integer.parseInt(s.replaceAll("^.*=",""));
//							System.out.println("setting maximum = max");
							mProgressBar.setMaximum(max);
							mProgressBar.setIndeterminate(false);
							mProgressBar.setStringPainted(true);  // show progress as percent
						} else if (s.contains("SHOWPROGRESS: Current")) {
							int val = Integer.parseInt(s.replaceAll("^.*=",""));
//							System.out.println("setting value = "+val);
							mProgressBar.setValue(val);
							mProgressBar.setIndeterminate(false);
						}
					}
				});
			} catch (Exception e) {
				e.printStackTrace();  // just in case something goes wrong
			}
		}
	}
    }

}*/
