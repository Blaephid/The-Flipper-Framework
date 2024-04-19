using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.TextCore.Text;
using Unity.VisualScripting;

public class S_HintBox : MonoBehaviour
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	public static S_HintBox	s_Instance;
	public TextMeshProUGUI	_HintText;
	public Animator		_BoxAnimator;
	[HideInInspector] 
	public GameObject	_CurrentHintRing;
	#endregion

	// Trackers
	#region trackers
	public float	_textDelay;
	public int	_fadeSpread;

	public bool	_isCurrentlyShowing { get; protected set; }
	private int	_currentPage = 0;
	private bool	_willTurnPage;
	private bool        _isCurrentlyOutOfRange;
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {

		//There can only be one instance of this script in game at once, so if this is the first spawned, set it as said instance, and delete it if it is not.
		if (!s_Instance) s_Instance = this;
		else { Destroy(gameObject); }

		//Makes text invisible.
		Color initialColor = _HintText.color;
		initialColor.a = 0;
		_HintText.color = initialColor;

		gameObject.SetActive(false); //Ensures is not visible on start
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//Called when a new hint becomes active to go through and display. This is the array of pages of hints, the array of each page's duration, and the object itself.
	public void ShowHint ( string[] hint, float[] duration) {
		//gameObject.SetActive(false);

		_isCurrentlyOutOfRange = true; //Rather than ending the current coroutine immediately, this allows it to end itself as its while loop becomes false.
		_willTurnPage = false; //Ensures current hint won't go onto next page.
		_HintText.text = ""; //Removes current text in hint box so it can replace it with hint.

		//Resets hint to start of array
		_currentPage = 0;

		gameObject.SetActive(true); //This should be attached to the hintBox object itself, so the UI objects should be children, thus becoming visible.
		_BoxAnimator.SetBool("Active", true); //To make appearance not instant and jarring.

		StartCoroutine(DisplayPage(hint[0], duration[0], hint, duration));
	}

	//The coroutine goes through all text on the page, slowly making it visible, then turning the page after a delay when done. As such, takes arrays and current elements of said arrays.
	IEnumerator DisplayPage ( string pageText, float duration, string[] fullText, float[] fullDur) {

		yield return new WaitForFixedUpdate(); //Allows one frame to finish any previous hints being displayed.

		_willTurnPage = false;

		_isCurrentlyShowing = true;
		_HintText.text = pageText; //Sets text box to display current page, but won't all be visible yet.

		_HintText.ForceMeshUpdate();
		TMP_TextInfo textInfo = _HintText.textInfo;
		Color32[] newVertexColors;

		//index and startCharacter determine the selection of characters that will have their alpha increased every loop.
		int startCharacter = 0; //Start character is the earliest one not fully visible, and will only go up when a new character becomes fully visible.
		int index = 0; //Index is the latest character in the selection, and goes up every loop, allowing multiple characters to have their alphas changed at once, but not all starting at the same time.
		int Length = textInfo.characterCount;

		_isCurrentlyOutOfRange = false; //When this becomes true, will end hint showing.

		//This loop will store all characters not fully visible (as any from after startCharacter) and start increasing the visibiliy of a set (determiend by index
		while (!_isCurrentlyOutOfRange)
		{
			byte fadeSteps = (byte)Mathf.Max(1, 255 / _fadeSpread);

			//Each while loop will check the current and next characters on page.
			for (int i = startCharacter ; i < index + 1 ; i++)
			{
				//If this character is already fully visible, go to next character
				if (!textInfo.characterInfo[i].isVisible) continue;

				int materialIndex = textInfo.characterInfo[i].materialReferenceIndex; //The material applied to the current character
				newVertexColors = textInfo.meshInfo[materialIndex].colors32;

				int vertexIndex = textInfo.characterInfo[i].vertexIndex; //Vertex data of current character on page.

				//Increase visibility of current character.
				byte alpha = (byte)Mathf.Clamp(newVertexColors[vertexIndex].a + fadeSteps, 0, 255);
				newVertexColors[vertexIndex].a = alpha;
				newVertexColors[vertexIndex + 1].a = alpha;
				newVertexColors[vertexIndex + 2].a = alpha;
				newVertexColors[vertexIndex + 3].a = alpha;

				//Once character had been made fully visible
				if (alpha == 255)
				{
					//Next loop of while loop, will go to the next two characters.
					startCharacter += 1;

					//If next start character is beyond how many characters there, then this is the end of the page.
					if (startCharacter >= Length)
					{
						_HintText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32); //Ensure all characters are displayed.
						yield return new WaitForSeconds(duration); //Keep page dispalyed for x seconds.

						//If there is a following page, then once while loop is over, will use this same method to display that.
						_currentPage += 1;
						if (_currentPage < fullText.Length)
						{
							_willTurnPage = true;
						}

						_HintText.ForceMeshUpdate();
						_isCurrentlyOutOfRange = true; //End while loop.
					}
				}
			}

			_HintText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

			if (index + 1 < Length) index += 1; //StartCharacter only increases when a character becomes visible, this increases after every delay, allowing multiple characters to all start fading in at the same time, at the same speed but at different points.

			yield return new WaitForSeconds(_textDelay); //Delay set in editor.
		}

		//End hint.

		//If set to go to next page, call coroutine again.
		if (_willTurnPage)
		{
			gameObject.SetActive(true);
			StartCoroutine(DisplayPage(fullText[_currentPage], fullDur[_currentPage], fullText, fullDur)); //Recursion. Calls itself again so can display the next page using the same method.
			yield break; //End this instance of the coroutine.
		}
		//End Hint
		else
		{
			_BoxAnimator.SetBool("Active", false);
			gameObject.SetActive(false);
			_isCurrentlyShowing = false;

			GameObject TempObject = _CurrentHintRing;

			TempObject.SetActive(false);
			_CurrentHintRing = null; //Allows the hint to be activate again.
			TempObject.SetActive(true);
		}
	}
	#endregion
}
