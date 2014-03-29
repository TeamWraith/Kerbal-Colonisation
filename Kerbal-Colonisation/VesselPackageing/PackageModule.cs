using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class PackageModule : PartModule
{
	private Rect windowPos = new Rect();
	private Transform unPackTransform;

	[KSPField(isPersistant = true)]
	public int vesselPacked = 0;

	[KSPField(isPersistant = true)]
	public int packageType = 0; //0 = ALL, 1 = VAB, 2 <= SPH

	[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true)]
	public string vesselPack = "Empty";


	[KSPEvent(guiActive = true, guiName = "Pack a vessel", guiActiveEditor = true)]
	public void SelectVessel()
	{
		RenderingManager.AddToPostDrawQueue(0, OnDraw);
	}
	[KSPEvent(guiActive = true, guiName = "Unpack")]
	public void UnpackVessel()
	{
		part.decouple (1);
		unPackTransform = transform;
		Destroy (part.gameObject);

		ShipConstruction.PutShipToGround( ShipConstruction.LoadShip( vesselPack ),  unPackTransform );
	}

	public override void OnStart (StartState state)
	{
		base.OnStart (state);
		if (state == StartState.Editor) 
		{
			if (vesselPacked == 0 || vesselPack == null)
			{
				SelectVessel();
			}
		}
		else
			Events["SelectVessel"].guiActive = false;
	}

	public override void OnFixedUpdate ()
	{
		base.OnFixedUpdate ();
		if (vessel.checkLanded ())
			Events ["UnpackVessel"].guiActive = true;
		else
			Events ["UnpackVessel"].guiActive = false;
	}

	public override void OnAwake ()
	{
		base.OnAwake ();
		print (vesselPack);
	}

	private void OnDraw()
	{
		windowPos = GUILayout.Window(31, windowPos, OnWindow, "Select a vessel");
	}

	private string[] ListVessels(int VAB)
	{
//		ArrayList vesselList = new ArrayList ();
		
		string cwd = Path.Combine(Path.Combine(new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName, "saves"), 
		                                      HighLogic.fetch.GameSaveFolder) + "/Ships";
//		string[] files;

		if (VAB > 0) 
		{

			if (VAB == 1) 
			{
				cwd = cwd + "/VAB";
			} 
			else 
			{
				cwd = cwd + "/SPH";
			}

			return Directory.GetFiles (cwd, "*.craft");

		} else
			return Directory.GetFiles (cwd, "*.craft", SearchOption.AllDirectories);

//		foreach (string path in files)
//		{
//			vesselList.Add ( ShipConstruction.LoadShip(path) );
//		}
//
//		ShipConstruct[] vessels = ShipConstruct[vesselList.Count];
//		vesselList.CopyTo (vessels);
//
//		return vessels;

	}


	private void OnWindow(int windowId)
	{
		GUILayout.BeginVertical (GUILayout.Width (320f));
//		foreach (ShipConstruct ship in ListVessels(packageType)) 
//		{
//			if ( GUILayout.Button( new GUIContent ( ship.shipName, ShipConstruction.CreateSubassemblyIcon(ship, 64) ) ) )
//			{
//				vesselPack = ship.shipName; //HACK if anyone has ships named the same in the SPH and the VAB this might not work.
//				vesselPacked = 1;
//			}
//		}
		GUILayout.Space (16);

		GUILayout.BeginScrollView(  new Vector2(620,512), false, true, GUILayout.Height(512) );
		GUILayout.BeginVertical (GUILayout.Width (310) );
		foreach (string path in ListVessels(packageType) ) 
		{
			string fileName = Path.GetFileNameWithoutExtension (path);

			if ( GUILayout.Button( fileName ) )
			{
				Fields["VesselPack"].guiName = fileName;
				vesselPack = path; //HACK if anyone has ships named the same in the SPH and the VAB this might not work.
				vesselPacked = 1;
				GUILayout.EndVertical ();
				GUILayout.EndScrollView ();

				RenderingManager.RemoveFromPostDrawQueue(0, OnDraw);
			}
		}
		GUILayout.EndVertical ();
		GUILayout.EndScrollView ();

		GUILayout.Space (16);
		if (GUILayout.Button ("Close"))
			RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
		GUI.DragWindow ();
	}

}

