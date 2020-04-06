using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = System.Object;

namespace Assets.Scripts.Utils
{
    public static class Tools
    {
        public static long TotalSeconds(this DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
        public static void SetAndStretchToParentSize(this RectTransform _mRect, RectTransform _parent)
        {
            _mRect.transform.SetParent(_parent);
            _mRect.anchorMin = new Vector2(0f, 0f);
            _mRect.anchorMax = new Vector2(1f, 1f);
            _mRect.offsetMin = new Vector2(0f, 0f);
            _mRect.offsetMax = new Vector2(0f, 0f);
            _mRect.pivot = new Vector2(0.5f, 0.5f);
        }

        // Token: 0x06000CEA RID: 3306 RVA: 0x00039CB0 File Offset: 0x00037EB0
        public static void SetAndStretchToParentSizeWithMod(this RectTransform _mRect, RectTransform _parent, float mod)
        {
            _mRect.transform.SetParent(_parent, false);
            _mRect.anchorMin = new Vector2(0f, 0f);
            _mRect.anchorMax = new Vector2(1f, 1f);
            _mRect.pivot = new Vector2(0.5f, 0.5f);
            _mRect.offsetMin = new Vector2(mod, mod);
            _mRect.offsetMax = new Vector2(-mod, -mod);
        }

        // Token: 0x06000CEB RID: 3307 RVA: 0x00039D25 File Offset: 0x00037F25
        public static void DestroyChildren(this Transform transform)
        {
            transform.DestroyChildrenFromIndex(0);
        }

        // Token: 0x06000CEC RID: 3308 RVA: 0x00039D30 File Offset: 0x00037F30
        public static void DestroyChildrenImmediate(this Transform transform)
        {
            if (transform == null)
            {
                return;
            }
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        // Token: 0x06000CED RID: 3309 RVA: 0x00039D6C File Offset: 0x00037F6C
        public static void DestroyChildrenFromIndex(this Transform transform, int index)
        {
            if (transform == null)
            {
                return;
            }
            for (int i = transform.childCount - 1; i >= index; i--)
            {
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        // Token: 0x06000CEE RID: 3310 RVA: 0x00039DA8 File Offset: 0x00037FA8
        public static string GetPath(this Transform transform)
        {
            StringBuilder stringBuilder = new StringBuilder("/").Append(transform.name);
            while (transform.transform.parent != null)
            {
                transform = transform.parent;
                stringBuilder.Insert(0, transform.name).Insert(0, "/");
            }
            return stringBuilder.ToString();
        }
    }
}
