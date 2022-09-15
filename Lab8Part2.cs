///
/// IRP Module Programming
/// for Small Systems CST-225WY3 - Spring 2021
/// 
/// Programmer Name:  Dylan Wilmarth
/// 
/// Date: May 3, 2021
/// 
/// Revision History:
/// 
/// [0] - 05/18/18 - AGC - Program created, code adjusted for UDP client rather than TCP.      
/// 
/// [1] - 05/13/21 - DJW - Created new UdpClient (inputClient) to set specifically to IRP #0 module
/// to collect input. Inserted code to set all ports on chosen output file so all pins switch to
/// output.
/// 
/// [2] - 05/14/21 - DJW - Created switch statement to check what port the input was received from.
/// 

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
namespace IRPProgramming
{
    class IRPProgramming_OOnly
    {
        private const string hostName = "cst225.dyndns.org";
        private static int portNum;

        static UdpClient client;

        // Added to specifically take input bytes
        static UdpClient inputClient;

        static byte[] bytes = new byte[1024];

        static int Main(string[] args)
        {
            // Determine the output of an error message based on the command-line input of the port number
            if (args.Length == 0)
            {
                // Output an error message to the screen
                Console.WriteLine("\nPlease enter the IRP port number (2424-2427) when running IRP Program!");
                Console.WriteLine("\nFor example: C:> IRPProgramming 2424 <return>\n");

                // Exit the program
                return (0);
            }

            // Attempt to convert the string representing the port number to an integer if the input value is valid
            try
            {
                portNum = int.Parse(args[0]);
            }

            // Output an error message to the screen if the input value is invalid
            catch (System.FormatException)
            {
                // Output an error message to the screen
                Console.WriteLine("\nPlease enter the integer IRP port number (2424-2427) when running IRPProgramming!");
                Console.WriteLine("\nFor example: C:> IRPProgramming 2424 <return>\n");

                // Exit the program
                return (0);
            }

            // Attempt to illuminate the seven-segment display LEDs over the network if the client info is valid
            try
            {
                // Set up the network client with the host name and port number specified earlier
                client = new UdpClient(hostName, portNum);
                inputClient = new UdpClient(hostName, 2423);

                // Output a successful connection message to the screen
                Console.WriteLine("\r\nConnection Established to the IRP module in AT002A...\r\n");
                Console.WriteLine("Light up a remote LED on the selected device...\n");


                /// Add all code that will be used to illuminate the IRP lights here

                /// Create variable encase different input ports are used
                char inputPort;

                // Create IPEndPoint to receive data from IRP module
                System.Net.IPEndPoint remoteEP = null;

                /// Set all ports on selected module to output
                // Set A port to input
                bytes[0] = (byte)'!';
                bytes[1] = (byte)'A';
                bytes[2] = (byte)1;
                client.Send(bytes, 3);

                // Set B port to output
                bytes[0] = (byte)'!';
                bytes[1] = (byte)'B';
                bytes[2] = (byte)0;
                client.Send(bytes, 3);

                // Set C port to output
                bytes[0] = (byte)'!';
                bytes[1] = (byte)'C';
                bytes[2] = (byte)0;
                client.Send(bytes, 3);

                for (char index = 'A'; index < 'D'; index++)
                {

                    // Send to IRP module for input
                    bytes[0] = (byte)'c';
                    inputClient.Send(bytes, 1);

                    // Read in input data
                    byte[] receiveBytes = inputClient.Receive(ref remoteEP);

                    // Switch ASCII value received to character for console output
                    switch ((int)receiveBytes[0])
                    {
                        case 65:
                            inputPort = 'a';
                            break;
                        case 66:
                            inputPort = 'b';
                            break;
                        default:
                            inputPort = 'c';
                            break;
                    }
                    // Write to console
                    Console.WriteLine("Bytes Received from IRP module\nPort:{0} Data:{1}", inputPort, receiveBytes[1]);

                    // After 2 seconds of all LEDs on, turn all LEDs off
                    System.Threading.Thread.Sleep(2000);

                    // Write to console
                    Console.WriteLine("Outputting {0} on port {1}\n", receiveBytes[1], index);

                    // Output received data to port
                    Output(receiveBytes[1], index);

                    // After 2 seconds of all LEDs on, turn all LEDs off
                    System.Threading.Thread.Sleep(3000);
                }

                // Turn off LED on the IRP module
                Output(0, 'A');

                // Turn off LED on the IRP module
                Output(0, 'B');

                // Turn off LED on the IRP module
                Output(0, 'C');

                // Output a program completion message to the screen
                Console.WriteLine("ALL LEDs Off... ");

                return 0;
            }

            // Output an error message to the screen and close the client if the information provided is invalid
            catch
            {
                // Output an error message to the screen
                Console.WriteLine("\nIRP Module - Network Error\n");

                // Exit the UDP client
                client.Close();

                // Return an error code to the system
                return 1;
            }
        }

        //------------------------------------------------------------------------------------------------------------
        //  Output - Sends a value over the network to illuminate any combination of LEDs on the IRP module.
        //------------------------------------------------------------------------------------------------------------
        static void Output(int value, char port)
        {
            // Configure the data to be sent to a port
            bytes[0] = (byte)port;
            bytes[1] = (byte)value;

            // Attempt to write the values in bytes[] over the network if the start/end points are valid
            try
            {
                client.Send(bytes, 2);
            }

            // Output an error message to the screen if the start/end points are invalid
            catch
            {
                Console.WriteLine("IRP Module - Network Error\n");
            }
        }
    }
}
