using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Slot3x3.EditorTools
{
    /// <summary>Small helpers shared by the scene builder: creating sprites, text, UI elements and wiring serialized fields.</summary>
    public static class BuilderHelpers
    {
        public static readonly Color HudPurple = new Color(0.655f, 0.369f, 0.949f, 1f);

        /// <summary>The font that ships with every Unity install.</summary>
        public static Font BuiltInFont
        {
            get { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
        }

        public static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) Debug.LogError("Missing asset: " + path);
            return asset;
        }

        /// <summary>
        /// Loads a sprite by its logical art path. All art is drawn from code, so the path only names
        /// which placeholder to use (see <see cref="PlaceholderArt"/>).
        /// </summary>
        public static Sprite LoadSprite(string path)
        {
            return PlaceholderArt.Resolve(path);
        }

        #region world objects
        public static GameObject Child(string name, Transform parent, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go;
        }

        public static SpriteRenderer SpriteObject(string name, Transform parent, Vector3 localPosition, Sprite sprite, int sortingOrder)
        {
            GameObject go = Child(name, parent, localPosition);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        public static TextMesh TextObject(string name, Transform parent, Vector3 localPosition, string text, Font font, Material material,
                                          float characterSize, TextAnchor anchor, TextAlignment alignment, Color color, int sortingOrder)
        {
            GameObject go = Child(name, parent, localPosition);
            var tm = go.AddComponent<TextMesh>();
            tm.font = font;
            tm.text = text;
            tm.fontSize = 50;
            tm.characterSize = characterSize;
            tm.anchor = anchor;
            tm.alignment = alignment;
            tm.color = color;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = material;
            mr.sortingOrder = sortingOrder;
            return tm;
        }

        public static SceneButton ButtonObject(string name, Transform parent, Vector3 localPosition, Sprite normal, Sprite pressed,
                                               Vector2 colliderSize, Vector2 colliderOffset, bool trigger, int sortingOrder,
                                               bool toggle = false, bool clickOnPress = false)
        {
            SpriteRenderer sr = SpriteObject(name, parent, localPosition, normal, sortingOrder);
            var box = sr.gameObject.AddComponent<BoxCollider2D>();
            box.size = colliderSize;
            box.offset = colliderOffset;
            box.isTrigger = trigger;

            var button = sr.gameObject.AddComponent<SceneButton>();
            button.SetSprites(normal, pressed);
            Set(button, "toggle", toggle);
            Set(button, "clickOnPress", clickOnPress);
            return button;
        }
        #endregion world objects

        #region ui
        public static RectTransform UIRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
                                           Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
            return rt;
        }

        /// <summary>A rect centred in its parent with a fixed size.</summary>
        public static RectTransform UICentered(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            return UIRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size);
        }

        public static RectTransform UIStretched(string name, Transform parent)
        {
            return UIRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        public static Image UIImage(RectTransform rect, Sprite sprite, bool raycast = false)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = raycast;
            return image;
        }

        public static Text UIText(RectTransform rect, string text, Font font, int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter, Material material = null)
        {
            var t = rect.gameObject.AddComponent<Text>();
            t.text = text;
            t.font = font;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = alignment;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            if (material != null) t.material = material;
            return t;
        }
        #endregion ui

        #region serialized fields
        /// <summary>Sets a (private) serialized field of a component by name.</summary>
        public static void Set(Object target, string property, object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(property);
            if (p == null)
            {
                Debug.LogError("No serialized field '" + property + "' on " + target.GetType().Name);
                return;
            }

            switch (value)
            {
                case Object o: p.objectReferenceValue = o; break;
                case int i: p.intValue = i; break;
                case float f: p.floatValue = f; break;
                case bool b: p.boolValue = b; break;
                case string s: p.stringValue = s; break;
                case Vector3 v: p.vector3Value = v; break;
                case int[] ints:
                    p.arraySize = ints.Length;
                    for (int k = 0; k < ints.Length; k++) p.GetArrayElementAtIndex(k).intValue = ints[k];
                    break;
                case System.Array array:
                    p.arraySize = array.Length;
                    for (int k = 0; k < array.Length; k++)
                        p.GetArrayElementAtIndex(k).objectReferenceValue = (Object)array.GetValue(k);
                    break;
                default:
                    Debug.LogError("Unsupported value for " + property);
                    break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        #endregion serialized fields
    }
}
