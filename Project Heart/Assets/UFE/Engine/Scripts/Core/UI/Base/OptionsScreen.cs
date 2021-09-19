using UnityEngine;
using System;
using System.Reflection;

namespace UFE3D
{
	public class OptionsScreen : UFEScreen
	{
		public virtual float GetMusicVolume()
		{
			return UFE.GetMusicVolume();
		}

		public virtual float GetSoundFXVolume()
		{
			return UFE.GetSoundFXVolume();
		}

		public virtual void GoToControlsScreen()
		{
			if (UFE.config.inputOptions.inputManagerType == InputManagerType.cInput && UFE.isCInputInstalled)
			{
				UFE.SearchClass("cGUI").GetMethod(
					"ToggleGUI",
					BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					null,
					new Type[] { },
					null
				).Invoke(null, null);

			}
			else if (UFE.config.inputOptions.inputManagerType == InputManagerType.Rewired && UFE.isRewiredInstalled)
			{
				if (RewiredInputController.inputConfiguration != null)
				{
					RewiredInputController.inputConfiguration.ShowInputConfigurationUI(() =>
					{

						// Enable interaction again after closed
						if (gameObject.GetComponent<CanvasGroup>() != null) gameObject.GetComponent<CanvasGroup>().interactable = true;
					});
					// Prevent interaction on the screen while Control Mapper is open
					CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
					if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
					canvasGroup.interactable = false;
				}
			}
		}

		public virtual void GoToMainMenuScreen()
		{
			UFE.StartMainMenuScreen();
		}

		public virtual bool IsMusicMuted()
		{
			return !UFE.config.music;
		}

		public virtual bool IsSoundMuted()
		{
			return !UFE.config.soundfx;
		}

		public void MuteMusic(bool mute)
		{
			this.SetMusic(!mute);
		}

		public void MuteSoundFX(bool mute)
		{
			this.SetSoundFX(!mute);
		}

		public virtual void SetAIDifficulty(AIDifficultySettings difficulty)
		{
			if (difficulty != null)
			{
				UFE.SetAIDifficulty(difficulty.difficultyLevel);
			}
		}

		public virtual void SetAIEngine(AIEngine aiEngine)
		{
			if (UFE.isAiAddonInstalled)
			{
				UFE.SetAIEngine(aiEngine);
			}
			else
			{
				UFE.SetAIEngine(AIEngine.RandomAI);
			}
		}

		public virtual void SetDebugMode(bool enabled)
		{
			UFE.SetDebugMode(enabled);
		}

		public virtual void SetMusic(bool enabled)
		{
			UFE.SetMusic(enabled);
		}

		public virtual void SetSoundFX(bool enabled)
		{
			UFE.SetSoundFX(enabled);
		}

		public virtual void SetMusicVolume(float volume)
		{
			UFE.SetMusicVolume(volume);
		}

		public virtual void SetSoundFXVolume(float volume)
		{
			UFE.SetSoundFXVolume(volume);
		}

		public virtual void ToggleAIEngine()
		{
			if (UFE.GetAIEngine() == AIEngine.RandomAI)
			{
				this.SetAIEngine(AIEngine.FuzzyAI);
			}
			else
			{
				this.SetAIEngine(AIEngine.RandomAI);
			}
		}

		public virtual void ToggleDebugMode()
		{
			this.SetDebugMode(!UFE.config.debugOptions.debugMode);
		}

		public virtual void ToggleMusic()
		{
			UFE.SetMusic(!UFE.GetMusic());
		}

		public virtual void ToggleSoundFX()
		{
			UFE.SetSoundFX(!UFE.GetSoundFX());
		}
	}
}