using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using DorfRomantiCam.Properties;
using HarmonyLib;
using UnityEngine;
using Dorfromantik;
using Random = UnityEngine.Random;

namespace DorfRomantiCam
{
	// Token: 0x02000003 RID: 3
	[BepInPlugin("org.bepinex.plugins.MatSch", "DorfMod", "1.0.0.0")]
	public class DorfMod : BaseUnityPlugin
	{
		// Token: 0x06000007 RID: 7 RVA: 0x00002121 File Offset: 0x00000321
		internal void Awake()
		{
			this.harmony = new Harmony("com.mat.DorfMod");
			this.mainCam = Camera.main;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002140 File Offset: 0x00000340
		private static AssetBundle LoadAssetBundle(byte[] resourceBytes)
		{
			bool flag = resourceBytes == null;
			if (flag)
			{
				throw new ArgumentNullException("resourceBytes");
			}
			return AssetBundle.LoadFromMemory(resourceBytes);
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002170 File Offset: 0x00000370
		public void Start()
		{
			this.normalRotation = Camera.main.transform.rotation;
			AssetBundle assetBundle = DorfMod.LoadAssetBundle(DorfRomantiCam.Properties.Resources.shading);
			AssetBundle assetBundle2 = DorfMod.LoadAssetBundle(DorfRomantiCam.Properties.Resources.wind);
			this.tiletile = GameObject.FindObjectsOfType<Tile>();
			this.windObj = assetBundle2.LoadAsset<GameObject>("Wind");
			this.FresnelMat = new Material(assetBundle.LoadAsset<Material>("Fresnel").shader);
			this.mainCam = Camera.main;
			MethodInfo methodInfo = AccessTools.Method(typeof(CameraZoom), "Start", null, null);
			MethodInfo method = typeof(ZoomPatch).GetMethod("ZoomPatchMethod");
			this.harmony.Patch(methodInfo, new HarmonyMethod(method), null, null, null, null);
			MethodInfo method2 = typeof(VehicleDriver).GetMethod("StartMoving");
			MethodInfo method3 = typeof(VehiclePatch).GetMethod("VehiclePatchMethod");
			this.harmony.Patch(method2, new HarmonyMethod(method3), null, null, null, null);
			base.StartCoroutine(this.SpawnWind());
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002280 File Offset: 0x00000480
		private IEnumerator SpawnWind()
		{
			this.tiletile = GameObject.FindObjectsOfType<Tile>();
			int num;
			for (int i = 0; i < 20; i = num + 1)
			{
				Vector3 rnd = this.tiletile[Random.Range(0, this.tiletile.Length - 1)].transform.position;
				bool flag = rnd.y >= 0f;
				if (flag)
				{
					Quaternion rndRot = Random.rotation;
					GameObject obj = GameObject.Instantiate<GameObject>(this.windObj, rnd, Quaternion.Euler(0f, 0f, rndRot.eulerAngles.z));
					obj.transform.localScale /= (float)Random.Range(5, 10);
					ParticleSystem.MainModule particleMain = obj.GetComponent<ParticleSystem>().main;
					particleMain.loop = false;
					particleMain.duration = 15f;
					particleMain.stopAction = ParticleSystemStopAction.Destroy;
					rndRot = default(Quaternion);
					obj = null;
					particleMain = default(ParticleSystem.MainModule);
				}
				yield return new WaitForSeconds(Random.Range(0.5f, 7f));
				rnd = default(Vector3);
				num = i;
			}
			base.StartCoroutine(this.SpawnWind());
			yield break;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002290 File Offset: 0x00000490
		public void Update()
		{
			bool flag = Input.GetKeyDown(KeyCode.T) && this.atVehicle;
			if (flag)
			{
				Camera.main.transform.rotation = this.normalRotation;
				this.atVehicle = false;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				this.lastVehicle.GetComponent<SphereCollider>().enabled = true;
			}
			bool flag2 = !this.atVehicle;
			if (flag2)
			{
				Ray ray = this.mainCam.ScreenPointToRay(Input.mousePosition);
				RaycastHit raycastHit;
				Physics.Raycast(ray, out raycastHit, 1000f);
				GameObject gameObject = raycastHit.collider.gameObject;
				bool flag3 = gameObject != null && this.VehicleNames.Contains(gameObject.name);
				if (flag3)
				{
					this.lastVehicleMat = gameObject.GetComponentInChildren<MeshRenderer>().material;
					gameObject.GetComponentInChildren<MeshRenderer>().materials = new Material[]
					{
						this.lastVehicleMat,
						this.FresnelMat
					};
					this.lastVehicle = gameObject;
					this.normalRotation = Camera.main.transform.rotation;
					bool mouseButtonDown = Input.GetMouseButtonDown(0);
					if (mouseButtonDown)
					{
						this.lastVehicle.GetComponent<SphereCollider>().enabled = false;
						this.atVehicle = true;
						Material[] materials = new Material[]
						{
							this.lastVehicleMat
						};
						this.lastVehicle.GetComponentInChildren<MeshRenderer>().materials = materials;
						Cursor.lockState = CursorLockMode.Locked;
						Cursor.visible = false;
					}
				}
				else
				{
					bool flag4 = !this.VehicleNames.Contains(gameObject.name);
					if (flag4)
					{
						Material[] materials2 = new Material[]
						{
							this.lastVehicleMat
						};
						this.lastVehicle.GetComponentInChildren<MeshRenderer>().materials = materials2;
					}
				}
			}
			bool flag5 = this.atVehicle;
			if (flag5)
			{
				this.mainCam.transform.position = new Vector3(this.lastVehicle.transform.position.x, this.lastVehicle.transform.position.y + 0.1f, this.lastVehicle.transform.position.z);
				this.mainCam.transform.forward = this.lastVehicle.transform.forward;
			}
		}

		// Token: 0x04000003 RID: 3
		private Quaternion normalRotation;

		// Token: 0x04000004 RID: 4
		public List<string> VehicleNames = new List<string>();

		// Token: 0x04000005 RID: 5
		private GameObject lastVehicle;

		// Token: 0x04000006 RID: 6
		private Material lastVehicleMat;

		// Token: 0x04000007 RID: 7
		private Material FresnelMat;

		// Token: 0x04000008 RID: 8
		private GameObject windObj;

		// Token: 0x04000009 RID: 9
		private bool atVehicle = false;

		// Token: 0x0400000A RID: 10
		public Camera mainCam;

		// Token: 0x0400000B RID: 11
		public Transform startingPos;

		// Token: 0x0400000C RID: 12
		private Harmony harmony;

		// Token: 0x0400000D RID: 13
		private Tile[] tiletile;
	}

	// Token: 0x02000005 RID: 5
	[HarmonyPatch(typeof(VehicleDriver), "StartMoving")]
	public class VehiclePatch
	{
		// Token: 0x0600000F RID: 15 RVA: 0x00002580 File Offset: 0x00000780
		[HarmonyPrefix]
		public static void VehiclePatchMethod(VehicleDriver __instance)
		{
			GameObject gameObject = __instance.gameObject;
			SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
			sphereCollider.radius /= 2f;
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;
			DorfMod dorfMod = GameObject.FindObjectOfType<DorfMod>();
			bool flag = !dorfMod.VehicleNames.Contains(gameObject.name);
			if (flag)
			{
				dorfMod.VehicleNames.Add(gameObject.name);
			}
		}
	}

	// Token: 0x02000004 RID: 4
	[HarmonyPatch(typeof(CameraZoom))]
	public class ZoomPatch
	{
		// Token: 0x0600000D RID: 13 RVA: 0x000024F4 File Offset: 0x000006F4
		[HarmonyPrefix]
		public static void ZoomPatchMethod(CameraZoom __instance)
		{
			Camera.main.nearClipPlane = 0.0001f;
			Camera.main.farClipPlane = 10000f;
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = typeof(CameraZoom).GetField("zoomInDistance", bindingAttr);
			FieldInfo field2 = typeof(CameraZoom).GetField("zoomOutDistance", bindingAttr);
			field.SetValue(__instance, 55f);
			field2.SetValue(__instance, -100f);
		}
	}
}
