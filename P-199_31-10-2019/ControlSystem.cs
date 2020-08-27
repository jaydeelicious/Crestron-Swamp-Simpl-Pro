using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.AudioDistribution;         // For Swamp and Expanders
using Crestron.SimplSharpPro.EthernetCommunication;     // For EISC

namespace P_199_First_Take {
    public class ControlSystem : CrestronControlSystem {
        // Define Devices and Stuff

        private Swamp24x8 swamp;

        private AmplifiedZone[] p199zones;

        private EthernetIntersystemCommunications swampeisc;

        private SwampE8 swampexp1;
        private SwampE8 swampexp2;
        private SwampE8 swampexp3;

        private uint[] screens;
        private ushort[] sources;

        private uint groupmas;
        private bool[] grouped;

        public ControlSystem()
            : base() {
            try {
                Thread.MaxNumberOfUserThreads = 20;

                // Register the Swamp, the Zones and the EISC

                // Swamp and Expanders Construction

                swamp = new Swamp24x8(0x41, this);
                swamp.Description = "~~ P-199 Swamp ~~";
                swamp.ZoneChangeEvent += new ZoneEventHandler(swamp_ZoneChangeEvent);
                swamp.Register();

                swampexp1 = new SwampE8(1, swamp);
                swampexp2 = new SwampE8(2, swamp);
                swampexp3 = new SwampE8(3, swamp);
                swampexp1.ZoneChangeEvent += new ZoneEventHandler(swampexp1_ZoneChangeEvent);
                swampexp2.ZoneChangeEvent += new ZoneEventHandler(swampexp2_ZoneChangeEvent);
                swampexp3.ZoneChangeEvent += new ZoneEventHandler(swampexp3_ZoneChangeEvent);

                // Zones Definition

                p199zones = new AmplifiedZone[24];

                p199zones[0] = swamp.Zones[1];          // Wine Bar
                p199zones[1] = swamp.Zones[2];          // Games Room
                p199zones[2] = swamp.Zones[5];          // Pool Area
                p199zones[3] = swamp.Zones[8];          // Gym
                p199zones[4] = swampexp1.Zones[1];      // Massage Room
                p199zones[5] = swampexp1.Zones[3];      // Hers Dressing
                p199zones[6] = swampexp1.Zones[4];      // Office
                p199zones[7] = swampexp1.Zones[5];      // Kitchen
                p199zones[8] = swampexp1.Zones[6];      // Entrance Hall
                p199zones[9] = swampexp1.Zones[7];      // Living Area
                p199zones[10] = swampexp2.Zones[2];     // Dining Room
                p199zones[11] = swampexp2.Zones[3];     // Garden Room
                p199zones[12] = swampexp2.Zones[5];     // Hers Ensuite
                p199zones[13] = swampexp2.Zones[6];     // Master Bedroom
                p199zones[14] = swampexp2.Zones[7];     // His Dressing
                p199zones[15] = swampexp2.Zones[8];     // His Ensuite
                p199zones[16] = swampexp3.Zones[1];     // Outdoor Pool
                p199zones[17] = swampexp3.Zones[2];     // Terrace
                p199zones[18] = swampexp3.Zones[3];     // Subwoofer
                p199zones[19] = swampexp3.Zones[4];     // Patio
                p199zones[20] = swampexp3.Zones[5];     // Bed 2
                p199zones[21] = swampexp3.Zones[6];     // Bed 2 Ensuite
                p199zones[22] = swampexp3.Zones[7];     // Bed 3
                p199zones[23] = swampexp3.Zones[8];     // Bed 3 Ensuite

                swamp.Zones[1].Name.StringValue = "Wine Bar";
                swamp.Zones[2].Name.StringValue = "Games Room";
                swamp.Zones[5].Name.StringValue = "Pool Area";
                swamp.Zones[8].Name.StringValue = "Gym";
                swampexp1.Zones[1].Name.StringValue = "Massage Room";
                swampexp1.Zones[3].Name.StringValue = "Hers Dressing";
                swampexp1.Zones[4].Name.StringValue = "Office";
                swampexp1.Zones[5].Name.StringValue = "Kitchen";
                swampexp1.Zones[6].Name.StringValue = "Entrance Hall";
                swampexp1.Zones[7].Name.StringValue = "Living Area";
                swampexp2.Zones[2].Name.StringValue = "Dining Room";
                swampexp2.Zones[3].Name.StringValue = "Garden Room";
                swampexp2.Zones[5].Name.StringValue = "Hers Ensuite";
                swampexp2.Zones[6].Name.StringValue = "Master Bedroom";
                swampexp2.Zones[7].Name.StringValue = "His Dressing";
                swampexp2.Zones[8].Name.StringValue = "His Ensuite";
                swampexp3.Zones[1].Name.StringValue = "Outdoor Pool";
                swampexp3.Zones[2].Name.StringValue = "Back Garden";
                swampexp3.Zones[3].Name.StringValue = "Outside Subwoofer";
                swampexp3.Zones[4].Name.StringValue = "Patio";
                swampexp3.Zones[5].Name.StringValue = "Bedroom 2";
                swampexp3.Zones[6].Name.StringValue = "Bedroom 2 Ensuite";
                swampexp3.Zones[7].Name.StringValue = "Bedroom 3";
                swampexp3.Zones[8].Name.StringValue = "Bedroom 3 Ensuite";

                // Sources Array for easier programming on Room Power On

                sources = new ushort[24];

                sources[0] = 20;
                sources[1] = 20;
                sources[2] = 19;
                sources[3] = 3;
                sources[4] = 6;
                sources[5] = 22;
                sources[6] = 1;
                sources[7] = 2;
                sources[8] = 18;
                sources[9] = 18;
                sources[10] = 18;
                sources[11] = 18;
                sources[12] = 22;
                sources[13] = 21;
                sources[14] = 23;
                sources[15] = 23;
                sources[16] = 24;
                sources[17] = 24;
                sources[18] = 24;
                sources[19] = 24;
                sources[20] = 17;
                sources[21] = 17;
                sources[22] = 5;
                sources[23] = 5;

                // Initialize Every Zone Volume to a non eardrum-fatal level

                for (uint i = 0; i <= 23; i++)
                    p199zones[i].Volume.UShortValue = p199zones[i].VolumeFeedback.UShortValue;

                // EISC Construction

                swampeisc = new EthernetIntersystemCommunications(0xB5, "127.0.0.2", this);
                swampeisc.Description = "~~ Swamp EISC ~~";
                swampeisc.SigChange += new SigEventHandler(swampeisc_SigChange);
                swampeisc.Register();

                // Cross Routing

                screens = new uint[20];
                for (uint i = 0; i <= 19; i++)
                    screens[i] = 33;

                groupmas = 33;

                grouped = new bool[24];
                for (uint i = 0; i <= 23; i++)
                    grouped[i] = false;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControlSystem_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControlSystem_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControlSystem_ControllerEthernetEventHandler);
            }
            catch (Exception e) {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }


        /* This is a function that refreshes the feedbacks for the cross matrix, we use it on the control signals on the screens */

        void RefreshTSFbs(uint x) {
            swampeisc.UShortInput[x + 1].UShortValue = p199zones[screens[x]].VolumeFeedback.UShortValue;
            swampeisc.UShortInput[x + 21].UShortValue = (ushort)(Math.Floor((double)(p199zones[screens[x]].VolumeFeedback.UShortValue) / 655));
            swampeisc.BooleanInput[x + 1].BoolValue = p199zones[screens[x]].MuteOnFeedback.BoolValue;
        }





        /* --- This is the handler for our EISC, if anyone wants to change the function of a signal (when it's high), here is the place to do it --- */
        // TIP: Every Button is Programmed for each Zone starting from the Zones of the main Swamp (Wine Bar etc.)
        //      and ending with the Zones from the third expander (Outdoor Pool, ..., Bed 3 Ensuite etc.)
        //      Check the Zone Definition above for more info . . .

        void swampeisc_SigChange(BasicTriList currentDevice, SigEventArgs args) {

            switch (args.Sig.Type) {        // Check the type of signal changing

                case eSigType.Bool:         // If the signal is digital

                    if (args.Sig.BoolValue) {           // If it is high 

                        if (args.Sig.Number > 0 && args.Sig.Number <= 480) {        // Control TSx -> Ry

                            if (args.Sig.Number > 0 && args.Sig.Number <= 24) {         // Control TS1 -> R1-24
                                screens[0] = args.Sig.Number - 1;
                                groupmas = args.Sig.Number - 1;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas;       
                                RefreshTSFbs(0);
                            }

                            else if (args.Sig.Number > 24 && args.Sig.Number <= 48) {           // Control TS2 -> R1-24
                                screens[1] = args.Sig.Number - 25;
                                groupmas = args.Sig.Number - 25;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas;       
                                RefreshTSFbs(1);
                            }

                            else if (args.Sig.Number > 48 && args.Sig.Number <= 72) {           // Control TS3 -> R1-24
                                screens[2] = args.Sig.Number - 49;
                                groupmas = args.Sig.Number - 49;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas;       
                                RefreshTSFbs(2);
                            }

                            else if (args.Sig.Number > 72 && args.Sig.Number <= 96) {           // Control TS4 -> R1-24
                                screens[3] = args.Sig.Number - 73;
                                groupmas = args.Sig.Number - 73;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas;       
                                RefreshTSFbs(3);
                            }

                            else if (args.Sig.Number > 96 && args.Sig.Number <= 120) {          // Control TS5 -> R1-24
                                screens[4] = args.Sig.Number - 97;
                                groupmas = args.Sig.Number - 97;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(4);
                            }

                            else if (args.Sig.Number > 120 && args.Sig.Number <= 144) {         // Control TS6 -> R1-24
                                screens[5] = args.Sig.Number - 121;
                                RefreshTSFbs(5);
                            }

                            else if (args.Sig.Number > 144 && args.Sig.Number <= 168) {         // Control TS7 -> R1-24
                                screens[6] = args.Sig.Number - 145;
                                groupmas = args.Sig.Number - 145;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(6);
                            }

                            else if (args.Sig.Number > 168 && args.Sig.Number <= 192) {         // Control TS8 -> R1-24
                                screens[7] = args.Sig.Number - 169;
                                groupmas = args.Sig.Number - 169;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(7);
                            }

                            else if (args.Sig.Number > 192 && args.Sig.Number <= 216) {         // Control TS9 -> R1-24
                                screens[8] = args.Sig.Number - 193;
                                groupmas = args.Sig.Number - 193;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(8);
                            }

                            else if (args.Sig.Number > 216 && args.Sig.Number <= 240) {         // Control TS10 -> R1-24
                                screens[9] = args.Sig.Number - 217;
                                groupmas = args.Sig.Number - 217;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(9);
                            }

                            else if (args.Sig.Number > 240 && args.Sig.Number <= 264) {         // Control TS11 -> R1-24
                                screens[10] = args.Sig.Number - 241;
                                groupmas = args.Sig.Number - 241;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(10);
                            }

                            else if (args.Sig.Number > 264 && args.Sig.Number <= 288) {         // Control TS12 -> R1-24
                                screens[11] = args.Sig.Number - 265;
                                groupmas = args.Sig.Number - 265;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(11);
                            }

                            else if (args.Sig.Number > 288 && args.Sig.Number <= 312) {         // Control TS13 -> R1-24
                                screens[12] = args.Sig.Number - 289;
                                groupmas = args.Sig.Number - 289;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(12);
                            }

                            else if (args.Sig.Number > 312 && args.Sig.Number <= 336) {         // Control TS14 -> R1-24
                                screens[13] = args.Sig.Number - 313;
                                groupmas = args.Sig.Number - 313;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(13);
                            }

                            else if (args.Sig.Number > 336 && args.Sig.Number <= 360) {         // Control TS15 -> R1-24
                                screens[14] = args.Sig.Number - 337;
                                groupmas = args.Sig.Number - 337;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(14);
                            }

                            else if (args.Sig.Number > 360 && args.Sig.Number <= 384) {         // Control TS16 -> R1-24
                                screens[15] = args.Sig.Number - 361;
                                groupmas = args.Sig.Number - 361;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(15);
                            }

                            else if (args.Sig.Number > 384 && args.Sig.Number <= 408) {         // Control TS17 -> R1-24
                                screens[16] = args.Sig.Number - 385;
                                groupmas = args.Sig.Number - 385;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(16);
                            }

                            else if (args.Sig.Number > 408 && args.Sig.Number <= 432) {         // Control TS18 -> R1-24
                                screens[17] = args.Sig.Number - 409;
                                groupmas = args.Sig.Number - 409;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(17);
                            }

                            else if (args.Sig.Number > 432 && args.Sig.Number <= 456) {         // Control TS19 -> R1-24
                                screens[18] = args.Sig.Number - 433;
                                groupmas = args.Sig.Number - 433;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(18);
                            }

                            else if (args.Sig.Number > 456 && args.Sig.Number <= 480) {         // Control TS20 -> R1-24
                                screens[19] = args.Sig.Number - 457;
                                groupmas = args.Sig.Number - 457;
                                swampeisc.UShortInput[89].UShortValue = (ushort)groupmas; 
                                RefreshTSFbs(19);
                            }
                        }

                        else if (args.Sig.Number > 480 && args.Sig.Number <= 500) {             // Disconnect TSx
                            screens[args.Sig.Number - 481] = 33;
                        }

                        else if (args.Sig.Number > 500 && args.Sig.Number <= 520) {             // TSx Room Power On 
                            p199zones[screens[args.Sig.Number - 501]].Source.UShortValue = sources[screens[args.Sig.Number - 501]];
                        }

                        else if (args.Sig.Number > 520 && args.Sig.Number <= 540) {             // TSx Room Power Off 
                            if (screens[args.Sig.Number - 521] != 33)
                                p199zones[screens[args.Sig.Number - 521]].Source.UShortValue = 0;
                        }

                        else if (args.Sig.Number > 540 && args.Sig.Number <= 560) {             // TSx Room Mute On/Off
                            if (screens[args.Sig.Number - 541] != 33) {
                                if (p199zones[screens[args.Sig.Number - 541]].MuteOnFeedback.BoolValue)
                                    p199zones[screens[args.Sig.Number - 541]].MuteOff();
                                else
                                    p199zones[screens[args.Sig.Number - 541]].MuteOn();
                            }
                        }

                        else if (args.Sig.Number > 560 && args.Sig.Number <= 580) {             // TSx Room Volume Up
                            if (screens[args.Sig.Number - 561] != 33) {
                                if (p199zones[screens[args.Sig.Number - 561]].VolumeFeedback.UShortValue < 64880)
                                    p199zones[screens[args.Sig.Number - 561]].Volume.UShortValue = (ushort)(p199zones[screens[args.Sig.Number - 561]].VolumeFeedback.UShortValue + (ushort)655);
                            }
                        }

                        else if (args.Sig.Number > 580 && args.Sig.Number <= 600) {             // TSx Room Volume Down
                            if (screens[args.Sig.Number - 581] != 33) {
                                if (p199zones[screens[args.Sig.Number - 581]].VolumeFeedback.UShortValue > 655)
                                    p199zones[screens[args.Sig.Number - 581]].Volume.UShortValue = (ushort)(p199zones[screens[args.Sig.Number - 581]].VolumeFeedback.UShortValue - (ushort)655);
                            }
                        }

                        else if (args.Sig.Number > 600 && args.Sig.Number <= 624) {             // Zones Power On
                            p199zones[args.Sig.Number - 601].Source.UShortValue = sources[args.Sig.Number - 601];
                        }

                        else if (args.Sig.Number > 625 && args.Sig.Number <= 648) {             // Zones Power Off
                            p199zones[args.Sig.Number - 625].Source.UShortValue = 0;
                        }

                        else if (args.Sig.Number > 648 && args.Sig.Number <= 672) {             // Zones Mute On/Off
                            if (p199zones[args.Sig.Number - 649].MuteOnFeedback.BoolValue)
                                p199zones[args.Sig.Number - 649].MuteOff();
                            else
                                p199zones[args.Sig.Number - 649].MuteOn();
                        }

                        else if (args.Sig.Number > 672 && args.Sig.Number <= 696) {             // Zones Volume Up
                            if (p199zones[args.Sig.Number - 673].VolumeFeedback.UShortValue < 64880)
                                p199zones[args.Sig.Number - 673].Volume.UShortValue = (ushort)(p199zones[args.Sig.Number - 673].VolumeFeedback.UShortValue + (ushort)655);
                        }

                        else if (args.Sig.Number > 696 && args.Sig.Number <= 720) {             // Zones Volume Down
                            if (p199zones[args.Sig.Number - 697].VolumeFeedback.UShortValue > 655)
                                p199zones[args.Sig.Number - 697].Volume.UShortValue = (ushort)(p199zones[args.Sig.Number - 697].VolumeFeedback.UShortValue - (ushort)655);
                        }

                        else if (args.Sig.Number > 720 && args.Sig.Number <= 744) {         // Zones Custom Bussing
                            if (groupmas != 33) {
                                p199zones[args.Sig.Number - 721].Source.UShortValue = sources[groupmas];
                                grouped[args.Sig.Number - 721] = true;
                            }
                        }

                        else if (args.Sig.Number > 744 && args.Sig.Number <= 768) {         // Zones Ungrouping
                            p199zones[args.Sig.Number - 745].Source.UShortValue = 0;
                            grouped[args.Sig.Number - 745] = false;
                        }

                        

                    }
                    break;          // break for case of digital signal


                case eSigType.UShort:              // If we got an analog signal

                    if (args.Sig.Number > 0 && args.Sig.Number <= 20) {             // TSx Volume Set
                        p199zones[screens[args.Sig.Number - 1]].Volume.UShortValue = args.Sig.UShortValue;
                    }

                    else if (args.Sig.Number > 20 && args.Sig.Number <= 44) {       // Zones Volume Set
                        p199zones[args.Sig.Number - 21].Volume.UShortValue = args.Sig.UShortValue;
                    }
                    break;          // break for case of analog signal
            }
        }





        /* --- This is the handler for the Swamp Expander No. 3, here we define the Feedbacks for p199zones[16] to p199zones[23] which correlate to the 8 Zones we get
                                                        from this Expander . . .                                                        --- */

        void swampexp3_ZoneChangeEvent(object sender, ZoneEventArgs args) {
            switch (args.EventId) {         // Specify what kind of event is triggered

                case ZoneEventIds.VolumeFeedbackEventId:            // If the Volume changes

                    for (uint i = 1; i <= 20; i++)              // Analog Outputs 1 - 20 are TSx Volume Feedback
                        if (screens[i - 1] >= 16 && screens[i - 1] <= 23)       // Swamp Expander 3 Controls p199zones[16] to p199zones[23]
                            swampeisc.UShortInput[i].UShortValue = p199zones[screens[i - 1]].VolumeFeedback.UShortValue;

                    for (uint i = 21; i <= 40; i++)             // Analog Outputs 21 - 40 are TSx Volume % Feedback
                        if (screens[i - 21] >= 16 && screens[i - 21] <= 23)         // zones[16] to zones[23] here
                            swampeisc.UShortInput[i].UShortValue = (ushort)(Math.Floor((double)(p199zones[screens[i - 21]].VolumeFeedback.UShortValue) / 655));

                    for (uint i = 57; i <= 64; i++)              // Analog Outputs 57 - 64 are zones[16] to zones[23] Volume Feedback
                        swampeisc.UShortInput[i].UShortValue = p199zones[i - 41].VolumeFeedback.UShortValue;

                    for (uint i = 81; i <= 88; i++)             // Analog Outputs 81 - 88 are zones[16] to zones[23] Volume % Feedback
                        swampeisc.UShortInput[i].UShortValue = (ushort)(Math.Floor((double)(p199zones[i - 65].VolumeFeedback.UShortValue) / 655));

                    swampeisc.UShortInput[89].UShortValue = (ushort)groupmas;       // Analog Output 89 is Group Master

                    break;          // break for case of volume feedback event

                case ZoneEventIds.MuteOnFeedbackEventId:

                    for (uint i = 1; i <= 20; i++)              // Digital Outputs 1 - 20 are TSx Mute On Feedback
                        if (screens[i - 1] >= 16 && screens[i - 1] <= 23)         // Swamp Expander 3 Controls p199zones[16] to p199zones[23]
                            swampeisc.BooleanInput[i].BoolValue = p199zones[screens[i - 1]].MuteOnFeedback.BoolValue;

                    for (uint i = 57; i <= 64; i++)             // Digital Outputs 57 - 64 are zones[16] to zones[23] Mute On Feedback
                        swampeisc.BooleanInput[i].BoolValue = p199zones[i - 41].MuteOnFeedback.BoolValue;

                    break;          // break for case of mute feedback event

                case ZoneEventIds.SourceFeedbackEventId:

                    for (uint i = 21; i <= 40; i++)             // Digital Outputs 21 - 40 are TSx Room On Feedback
                        if (screens[i - 21] >= 16 && screens[i - 21] <= 23) {       // Swamp Expander 3 Controls p199zones[16] to p199zones[23]
                            if (p199zones[screens[i - 21]].SourceFeedback.UShortValue != 0)
                                swampeisc.BooleanInput[i].BoolValue = true;
                            else
                                swampeisc.BooleanInput[i].BoolValue = false;
                        }

                    for (uint i = 81; i <= 88; i++) {             // Digital Outputs 81 - 88 are zones[16] to zones[23] Room On Feedback
                        if (p199zones[i - 65].SourceFeedback.UShortValue != 0)
                            swampeisc.BooleanInput[i].BoolValue = true;
                        else
                            swampeisc.BooleanInput[i].BoolValue = false;
                    }

                    for (uint i = 105; i <= 112; i++) {           // Digital Outputs 105 - 112 are zones[16] to zones[23] Party On Feedback
                        if (p199zones[i - 89].SourceFeedback.UShortValue == 2)
                            swampeisc.BooleanInput[i].BoolValue = true;
                        else
                            swampeisc.BooleanInput[i].BoolValue = false;
                    }

                    for (uint i = 113; i <= 136; i++) {         // Digital Outputs 113 - 136 are Zones Grouping
                        swampeisc.BooleanInput[i].BoolValue = grouped[i - 113];
                    }

                    break;          // break for case of source feedback event
            }
        }





        /* --- This is the handler for the Swamp Expander No. 2, here we define the Feedbacks for p199zones[10] to p199zones[15] which correlate to the 6 Zones we get
                                                                from this Expander . . .                                                        --- */

        void swampexp2_ZoneChangeEvent(object sender, ZoneEventArgs args) {

            switch (args.EventId) {         // Specify what kind of event is triggered

                case ZoneEventIds.VolumeFeedbackEventId:            // If the Volume changes

                    for (uint i = 1; i <= 20; i++)              // Analog Outputs 1 - 20 are TSx Volume Feedback
                        if (screens[i - 1] >= 10 && screens[i - 1] <= 15)       // Swamp Expander 2 Controls p199zones[10] to p199zones[15]
                            swampeisc.UShortInput[i].UShortValue = p199zones[screens[i - 1]].VolumeFeedback.UShortValue;

                    for (uint i = 21; i <= 40; i++)             // Analog Outputs 21 - 40 are TSx Volume % Feedback
                        if (screens[i - 21] >= 10 && screens[i - 21] <= 15)         // zones[10] to zones[15] here
                            swampeisc.UShortInput[i].UShortValue = (ushort)(Math.Floor((double)(p199zones[screens[i - 21]].VolumeFeedback.UShortValue) / 655));

                    for (uint i = 51; i <= 56; i++)             // Analog Ouputs 51 - 56 are zones[10] to zones[15] Volume Feedback
                        swampeisc.UShortInput[i].UShortValue = p199zones[i - 41].VolumeFeedback.UShortValue;

                    for (uint i = 75; i <= 80; i++)            // Analog Outputs 75 - 80 are zones[10] to zones[15] Volume % Feedback
                        swampeisc.UShortInput[i].UShortValue = (ushort)(Math.Floor((double)(p199zones[i - 65].VolumeFeedback.UShortValue) / 655));

                    swampeisc.UShortInput[89].UShortValue = (ushort)groupmas;       // Analog Output 89 is Group Master

                    /* ---- TESTING OUTPUT -----*/

                    CrestronConsole.PrintLine("Something changed on Swamp Expander 2. Dining Room Volume is: {0}, {1}, TS1 is {2}", p199zones[10].VolumeFeedback.UShortValue, swampexp2.Zones[2].VolumeFeedback.UShortValue, screens[0]);



                    break;          // break of case of volume feedback event

                case ZoneEventIds.MuteOnFeedbackEventId:

                    for (uint i = 1; i <= 20; i++)              // Digital Outputs 1 - 20 are TSx Mute On Feedback
                        if (screens[i - 1] >= 10 && screens[i - 1] <= 15)         // Swamp Expander 2 Controls p199zones[10] to p199zones[15]
                            swampeisc.BooleanInput[i].BoolValue = p199zones[screens[i - 1]].MuteOnFeedback.BoolValue;

                    for (uint i = 51; i <= 56; i++)             // Digital Outputs 41 - 44 are zones[10] to zones[15] Mute On Feedback
                        swampeisc.BooleanInput[i].BoolValue = p199zones[i - 41].MuteOnFeedback.BoolValue;

                    break;          // break for case of mute feedback event

                case ZoneEventIds.SourceFeedbackEventId:

                    for (uint i = 21; i <= 40; i++)             // Digital Outputs 21 - 40 are TSx Room On Feedback
                        if (screens[i - 21] >= 10 && screens[i - 21] <= 15) {       // Swamp Expander 2 Controls p199zones[10] to p199zones[15]
                            if (p199zones[screens[i - 21]].SourceFeedback.UShortValue != 0)
                                swampeisc.BooleanInput[i].BoolValue = true;
                            else
                                swampeisc.BooleanInput[i].BoolValue = false;
                        }

                    for (uint i = 75; i <= 80; i++) {             // Digital Outputs 75 - 80 are zones[10] to zones[15] Room On Feedback
                        if (p199zones[i - 65].SourceFeedback.UShortValue != 0)
                            swampeisc.BooleanInput[i].BoolValue = true;
                        else
                            swampeisc.BooleanInput[i].BoolValue = false;
                    }

                    for (uint i = 99; i <= 104; i++) {           // Digital Outputs 99 - 104 are zones[10] to zones[15] Party On Feedback
                        if (p199zones[i - 89].SourceFeedback.UShortValue == 2)
                            swampeisc.BooleanInput[i].BoolValue = true;
                        else
                            swampeisc.BooleanInput[i].BoolValue = false;
                    }

                    for (uint i = 113; i <= 136; i++) {         // Digital Outputs 113 - 136 are Zones Grouping
                        swampeisc.BooleanInput[i].BoolValue = grouped[i - 113];
                    }

                    break;          // break for case of source feedback event
            }

        }





        /* --- This is the handler for the Swamp Expander No. 1, here we define the Feedbacks for p199zones[4] to p199zones[9] which correlate to the 6 Zones we get
                                                                from this Expander . . .                                                        --- */

        void swampexp1_ZoneChangeEvent(object sender, ZoneEventArgs args) {

            switch (args.EventId) {         // Specify what kind of event is triggered

                case ZoneEventIds.VolumeFeedbackEventId:            // If the Volume changes

                    for (uint i = 1; i <= 20; i++)              // Analog Outputs 1 - 20 are TSx Volume Feedback
                        if (screens[i - 1] >= 4 && screens[i - 1] <= 9)         // Swamp Expander 1 Controls p199zones[4] to p199zones[9]
                            swampeisc.UShortInput[i].UShortValue = p199zones[screens[i - 1]].VolumeFeedback.UShortValue;

                    for (uint i = 21; i <= 40; i++)             // Analog Outputs 21 - 40 are TSx Volume % Feedback
                        if (screens[i - 21] >= 4 && screens[i - 21] <= 9)         // zones[4] to zones[9] here
                            swampeisc.UShortInput[i].UShortValue = (ushort)(Math.Floor((double)(p199zones[screens[i - 21]].VolumeFeedback.UShortValue) / 655));

                    for (uint i = 45; i <= 50; i++)             // Analog Ouputs 45 - 50 are zones[4] to zones[9] Volume Feedback
                        swampeisc.UShortInput[i].UShortValue = p199zones[i - 41].VolumeFeedback.UShortValue;

                    for (uint i = 69; i <= 74; i++)            // Analog Outputs 69 - 74 are zones[4] to zones[9] Volume % Feedback
                        swampeisc.UShortInput[i].UShortValue = (ushort)(Math.Floor((double)(p199zones[i - 65].VolumeFeedback.UShortValue) / 655));

                    /* ---- TESTING OUTPUT -----*/

                    CrestronConsole.PrintLine("Something changed on Swamp Expander 1. Living Room Volume is: {0}, {1}, TS1 is {2}", p199zones[9].VolumeFeedback.UShortValue, swampexp1.Zones[7].VolumeFeedback.UShortValue, screens[0]);

                    break;          // break for case of volume feedback event

                case ZoneEventIds.MuteOnFeedbackEventId:

                    for (uint i = 1; i <= 20; i++)              // Digital Outputs 1 - 20 are TSx Mute On Feedback
                        if (screens[i - 1] >= 4 && screens[i - 1] <= 9)         // Swamp Expander 1 Controls zones[4] to zones[9]
                            swampeisc.BooleanInput[i].BoolValue = p199zones[screens[i - 1]].MuteOnFeedback.BoolValue;

                    for (uint i = 45; i <= 50; i++)             // Digital Outputs 45 - 50 are zones[4] to zones[9] Mute On Feedback
                        swampeisc.BooleanInput[i].BoolValue = p199zones[i - 41].MuteOnFeedback.BoolValue;

                    break;          // break for case of mute feedback event

                case ZoneEventIds.SourceFeedbackEventId:

                    for (uint i = 21; i <= 40; i++)             // Digital Outputs 21 - 40 are TSx Room On Feedback
                        if (screens[i - 21] >= 4 && screens[i - 21] <= 9) {       // Swamp Expander 1 Controls p199zones[4] to p199zones[9]
                            if (p199zones[screens[i - 21]].SourceFeedback.UShortValue != 0)
                                swampeisc.BooleanInput[i].BoolValue = true;
                            else
                                swampeisc.BooleanInput[i].BoolValue = false;
                        }

                    for (uint i = 69; i <= 74; i++) {             // Digital Outputs 69 - 74 are zones[4] to zones[9] Room On Feedback
                        if (p199zones[i - 65].SourceFeedback.UShortValue != 0)
                            swampeisc.BooleanInput[i].BoolValue = true;
                        else
                            swampeisc.BooleanInput[i].BoolValue = false;
                    }

                    for (uint i = 93; i <= 98; i++) {           // Digital Outputs 93 - 98 are zones[4] to zones[9] Party On Feedback
                        if (p199zones[i - 89].SourceFeedback.UShortValue == 2)
                            swampeisc.BooleanInput[i].BoolValue = true;
                        else
                            swampeisc.BooleanInput[i].BoolValue = false;
                    }

                    for (uint i = 113; i <= 136; i++) {         // Digital Outputs 113 - 136 are Zones Grouping
                        swampeisc.BooleanInput[i].BoolValue = grouped[i - 113];
                    }

                    break;          // break for case of source feedback event
            }
        }





        /* --- This is the handler for the Swamp, here we define the Feedbacks for p199zones[0] to p199zones[3] which correlate to the 4 Zones we get
                                                                directly from the Swamp . . .                                                   --- */

        void swamp_ZoneChangeEvent(object sender, ZoneEventArgs args) {

            switch (args.EventId) {         // Specify what kind of event is triggered

                case ZoneEventIds.VolumeFeedbackEventId:            // If the Volume changes

                    for (uint i = 1; i <= 20; i++)              // Analog Outputs 1 - 20 are TSx Volume Feedback
                        if (screens[i - 1] >= 0 && screens[i - 1] <= 3)         // Swamp Controls only p199zones[0] to p199zones[3]
                            swampeisc.UShortInput[i].UShortValue = p199zones[screens[i - 1]].VolumeFeedback.UShortValue;

                    for (uint i = 21; i <= 40; i++)             // Analog Outputs 21 - 40 are TSx Volume % Feedback
                        if (screens[i - 21] >= 0 && screens[i - 21] <= 3)         // zones[0] to zones[3] here
                            swampeisc.UShortInput[i].UShortValue = (ushort)(Math.Floor((double)(p199zones[screens[i - 21]].VolumeFeedback.UShortValue) / 655));

                    for (uint i = 41; i <= 44; i++)             // Analog Ouputs 41 - 44 are zones[0] to zones[3] Volume Feedback
                        swampeisc.UShortInput[i].UShortValue = p199zones[i - 41].VolumeFeedback.UShortValue;

                    for (uint i = 65; i <= 68; i++)            // Analog Outputs 65 - 68 are zones[0] to zones[3] Volume % Feedback
                        swampeisc.UShortInput[i].UShortValue = (ushort)(Math.Floor((double)(p199zones[i - 65].VolumeFeedback.UShortValue) / 655));

                    /* ---- TESTING OUTPUT -----*/

                    CrestronConsole.PrintLine("Something changed on Swamp. Games Room Volume is: {0}, {1}, TS1 is {2}", p199zones[1].VolumeFeedback.UShortValue, swamp.Zones[2].VolumeFeedback.UShortValue, screens[1]);

                    break;          // break for case of volume feedback event

                case ZoneEventIds.MuteOnFeedbackEventId:

                    for (uint i = 1; i <= 20; i++)              // Digital Outputs 1 - 20 are TSx Mute On Feedback
                        if (screens[i - 1] >= 0 && screens[i - 1] <= 3)         // Swamp Controls only p199zones[0] to p199zones[3]
                            swampeisc.BooleanInput[i].BoolValue = p199zones[screens[i - 1]].MuteOnFeedback.BoolValue;

                    for (uint i = 41; i <= 44; i++)             // Digital Outputs 41 - 44 are zones[0] to zones[3] Mute On Feedback
                        swampeisc.BooleanInput[i].BoolValue = p199zones[i - 41].MuteOnFeedback.BoolValue;

                    break;          // break for case of mute feedback event

                case ZoneEventIds.SourceFeedbackEventId:

                    for (uint i = 21; i <= 40; i++)             // Digital Outputs 21 - 40 are TSx Room On Feedback
                        if (screens[i - 21] >= 0 && screens[i - 21] <= 3) {       // Swamp Controls only p199zones[0] to p199zones[3]
                            if (p199zones[screens[i - 21]].SourceFeedback.UShortValue != 0)
                                swampeisc.BooleanInput[i].BoolValue = true;
                            else
                                swampeisc.BooleanInput[i].BoolValue = false;
                        }

                    for (uint i = 65; i <= 68; i++) {             // Digital Outputs 65 - 68 are zones[0] to zones[3] Room On Feedback
                        if (p199zones[i - 65].SourceFeedback.UShortValue != 0)
                            swampeisc.BooleanInput[i].BoolValue = true;
                        else
                            swampeisc.BooleanInput[i].BoolValue = false;
                    }

                    for (uint i = 89; i <= 92; i++) {           // Digital Outputs 89 - 92 are zones[0] to zones[3] Party On Feedback
                        if (p199zones[i - 89].SourceFeedback.UShortValue == 2)
                            swampeisc.BooleanInput[i].BoolValue = true;
                        else
                            swampeisc.BooleanInput[i].BoolValue = false;
                    }

                    for (uint i = 113; i <= 136; i++) {         // Digital Outputs 113 - 136 are Zones Grouping
                        swampeisc.BooleanInput[i].BoolValue = grouped[i - 113];
                    }

                    break;          // break for case of source feedback event
            }
        }









        /* ---------------------------- SIGNAL DICTIONARY FOR THIS PROGRAM ------------------------------------------- */
        /* DIGITAL: Inputs 1 - 24: Control TS1
         *          Inputs 25 - 48: Control TS2
         *          .
         *          .
         *          .
         *          Inputs 457 - 480: Control TS20
         *          Inputs 481 - 500: Disconnect TSx
         *          Inputs 501 - 520: TSx Room Power On
         *          Inputs 521 - 540: TSx Room Power Off
         *          Inputs 541 - 560: TSx Room Mute On/Off
         *          Inputs 561 - 580: TSx Room Volume Up
         *          Inputs 581 - 600: TSx Room Volume Down
         *          Inputs 601 - 624: Zones Power On
         *          Inputs 625 - 648: Zones Power Off
         *          Inputs 649 - 672: Zones Mute On/Off
         *          Inputs 673 - 696: Zones Volume Up
         *          Inputs 697 - 720: Zones Volume Down
         *          Inputs 721 - 744: Zones Bussing
         *          Inputs 745 - 768: Zones Ungroup
         *          
         *          Outputs 1 - 20: TSx Mute On Feedback
         *          Outputs 21 - 40: TSx Room On Feedback
         *          Outputs 41 - 64: Zones Mute On Feedback
         *          Outputs 65 - 88: Zones Power On Feedback
         *          Outputs 89 - 112: Zones Party On Feedback
         *          Outputs 113 - 136: Zones Group Feedback
         *          
         * ANALOG:  Inputs 1 - 20: TSx Volume Set
         *          Inputs 21 - 44: Zones Volume Set
         * 
         *          Outputs 1 - 20: TSx Volume Feedback
         *          Outputs 21 - 40: TSx Volume % Feedback
         *          Outputs 41 - 64: Zones Volume Feedback
         *          Outputs 65 - 88: Zones Volume % Feedback
         *          Output 89: Group Master
         *          
         * --------------------------------------------- ENDS HERE ----------------------------------------------------*/















        /* ------------------------ Don't Touch Please ----------------------------- */

        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem() {
            try {

            }
            catch (Exception e) {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// wich Ethernet adapter this event belongs to.
        /// </param>
        void ControlSystem_ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs) {
            switch (ethernetEventArgs.EthernetEventType) {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter) {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter) {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType) {
            switch (programStatusEventType) {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void ControlSystem_ControllerSystemEventHandler(eSystemEventType systemEventType) {
            switch (systemEventType) {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }
    }
}