/****************************************************************************
 *
 * Copyright (c) 2014 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/
#if (UNITY_EDITOR && !UNITY_EDITOR_LINUX) || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_IOS
#define CRIWARE_SAMPLELIST_SUPPORT_MIC
#endif

using System.Collections.Generic;

public static class Scene_00_SampleListData
{
	public const string Title = "< CRI Samples >";
	public static readonly Dictionary<string, string[,]> SceneDict = new Dictionary<string, string[,]>(){
		{"Atom_Basic",
			new string[,] {
				{"Scene_01_SimplePlayback",
					"A simple sample that plays a sound\nby clicking the cube."
				},
				{"Scene_02_3DPosition",
					"This sample demonstrates distance attenuation\nby 3D positioning for the helicopter."
				},
				{"Scene_03_AISAC",
					"This sample demonstrates parameter control\nby AISAC.\nMoving the slider changes the pitch."
				},
				{"Scene_04_ControlParameter",
					"This sample demonstrates parameter control\nby SoundSource.\nMoving the slider changes the tone."
				},
				{"Scene_05_Category",
					"This sample demonstrates parameter control\nby Category.\nMoving the slider changes the volume."
				},
				{"Scene_06_BlockPlayback",
					"This sample demonstrates block playback\nBy changing the block during playback,\nsound status changes."
				},
				{"Scene_07_Ingame_Pinball",
					"A pinball game-like sample.\nWhile sounds are played by script,\nthe In-game preview connection\nfrom CRI Atom Craft is possible."
				},
			}
		},
		{"Atom_Advanced",
			new string[,] {
				{"Scene_02_OverScenePlayback",
					"This sample switches scenes\nwithout interrupting the BGM.\nYou can switch scenes\nby clicking on the cube."
				},
#if UNITY_IOS
				{"Scene_03_PlaybackWithOtherAudio",
					"This sample mutes the BGM category\nwhen another application is playing sounds.\nThis sample assumes the system is iOS."
				},
#endif
#if CRIWARE_SAMPLELIST_SUPPORT_LATENCY_ESTIMATION
				{"Scene_05_EstimateSoundLatency",
					"This sample gets the estimated value of\nsound playback latency on Android\nThis sample assumes the system is Android."
				},
#endif
#if UNITY_2018_1_OR_NEWER
				{"Scene_06_TimelineExtension",
					"A simple sample for the timeline extension."
				},
#endif
				{"Scene_07_Transceiver",
					"A Sample demostrating the Transceiver function."
				},
			}
		},
		{"Atom_Script",
			new string[,] {
				{"ScriptSample01_ClickToPlay",
					"This sample plays a sound\nwhen an object is clicked."
				},
				{"ScriptSample02_SoundTest",
					"This sample displays a button\nfor each Cue in the CueSheet file(ACB file)."
				},
				{"ScriptSample03_TitleScene",
					"This sample cross-fades music\nwhen scenes are switched."
				},
				{"ScriptSample04_TitleScene",
					"This sample can start from any scene\nwhile controlling sounds across scenes."
				},
				{"ScriptSample05_LevelMeter",
					"This sample gets the volume in real time\nand visualizes it by the cube size."
				},
				{"ScriptSample06_OutputCapture",
					"This sample captures pcm output from player\nand visualizes sound wave."
				},
				{"ScriptSample07_InputCapture",
					"This sample captures audio input\nfrom microphones and visualizes sound wave."
				},
				{"ScriptSample08_SeamlessSequencePlayback",
					"This sample play pcm and cue\nby seamless sequence playback."
				},
				{"GameSample_Pinball",
					"Pinball game sample project\nfor CRI Atom Craft"
				},
			}
		},
		{"Atom_Expansion",
			new string[,] {
				{"Scene_01_SoundxR_Preview",
					"This sample demonstrates the effect of Sound xR."
				},
			}
		},
#if UNITY_2019_4_OR_NEWER
		{"Asset_Support",
			new string[,] {
				{"Scene_01_AssetPlayback",
					"Simple playback using CriAtomAcbAsset and CriAtomAwbAsset."
				},
				{"Scene_02_Addressables",
					"Instantiate and playback from Addressables."
				},
				{"Scene_03_Multilingual",
					"Multilingual voices using CriAtomAcbLocalizedAsset."
				},
				{"Scene_04_Timeline",
					"Sample for using CriAtomAssetClip with Timeline extension."
				},
				{"Scene_07_CustomDeployType",
					"Adding 'DeployType' by implement ICriAssetImplCreator."
				},
			}
		},
#endif
	};
}
