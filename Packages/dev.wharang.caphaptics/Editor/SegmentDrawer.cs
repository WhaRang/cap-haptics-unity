using CapHaptics.PatternTypes;
using UnityEditor;
using UnityEngine;

namespace CapHaptics.Editor
{
	/// <summary>
	/// Draws a waveform <see cref="HapticPatternAsset.Segment"/> as only the fields its type
	/// actually uses: Buzz → delay, duration, amplitude; Curve → delay, duration, a drawable
	/// envelope and its probe count. The default drawer would show all five fields on every
	/// row and leave the author guessing which ones matter.
	/// </summary>
	[CustomPropertyDrawer(typeof(HapticPatternAsset.Segment))]
	public sealed class SegmentDrawer : PropertyDrawer
	{
		private const float Pad = 2f;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var type = property.FindPropertyRelative("type");
			var line = EditorGUIUtility.singleLineHeight;
			var y = position.y;

			Rect Row(float height)
			{
				var rect = new Rect(position.x, y, position.width, height);
				y += height + Pad;
				return rect;
			}

			EditorGUI.PropertyField(Row(line), type);
			EditorGUI.indentLevel++;
			EditorGUI.PropertyField(Row(line), property.FindPropertyRelative("delayMs"));
			EditorGUI.PropertyField(Row(line), property.FindPropertyRelative("durationMs"));

			if ((HapticPatternAsset.SegmentType)type.enumValueIndex == HapticPatternAsset.SegmentType.Buzz)
			{
				EditorGUI.PropertyField(Row(line), property.FindPropertyRelative("amplitude"));
			}
			else
			{
				var envelope = property.FindPropertyRelative("envelope");
				envelope.animationCurveValue = EditorGUI.CurveField(
					Row(line * 3f),
					"envelope",
					envelope.animationCurveValue,
					Color.cyan,
					new Rect(0f, 0f, 1f, 1f));
				EditorGUI.PropertyField(Row(line), property.FindPropertyRelative("probes"));
			}

			EditorGUI.indentLevel--;
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var line = EditorGUIUtility.singleLineHeight;
			var type = (HapticPatternAsset.SegmentType)property.FindPropertyRelative("type").enumValueIndex;

			// type + delay + duration, then amplitude OR (3-line curve + probes).
			var rows = type == HapticPatternAsset.SegmentType.Buzz
				? 4f * line + 4f * Pad
				: 4f * line + line * 3f + 5f * Pad;
			return rows;
		}
	}
}
