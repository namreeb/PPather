
/*
  This file is part of PPather.

    PPather is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PPather is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with PPather.  If not, see <http://www.gnu.org/licenses/>.

*/



using System;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;
using Glider.Common.Objects;

using Pather;
using Pather.Graph;

/*
 *  Classes to move the toon around 
 * 
 * Mover is a raw mover controlling what keys are pushed to move around
 * 
 * EasyMover easier access to MoveAlonger
 * 
 * MoveAlonger has the main logic to run along a computed path
 *
 */

namespace Pather
{
	public class Mover
	{

		private bool runForwards = false;
		private bool runBackwards = false;
		private bool strafeLeft = false;
		private bool strafeRight = false;
		private bool rotateLeft = false;
		private bool rotateRight = false;

		private const float PI = (float)Math.PI;
		private GContext Context;

		public Mover(GContext Context)
		{
			this.Context = Context;
		}

		private GSpellTimer KeyT = new GSpellTimer(50);
		private bool old_runForwards = false;
		private bool old_runBackwards = false;
		private bool old_strafeLeft = false;
		private bool old_strafeRight = false;
		private bool old_rotateLeft = false;
		private bool old_rotateRight = false;


		void PushKeys()
		{

			if (old_runForwards != runForwards)
			{
				KeyT.Wait();
				KeyT.Reset();
				if (runForwards)
					PressKey("Common.Forward");
				else
					ReleaseKey("Common.Forward");
				//PPather.WriteLine("Forwards: " + runForwards);
			}
			/*

							if(runForwards)
							{
								GContext.Main.StartRun(); // 
							}
							else
							{
								GContext.Main.ReleaseRun(); // 
							}
			*/
			if (old_runBackwards != runBackwards)
			{
				KeyT.Wait();
				KeyT.Reset();
				if (runBackwards)
					PressKey("Common.Back");
				else
					ReleaseKey("Common.Back");
				//PPather.WriteLine("Backwards: " + runBackwards);
			}
			if (old_strafeLeft != strafeLeft)
			{
				KeyT.Wait();
				KeyT.Reset();
				if (strafeLeft)
					PressKey("Common.StrafeLeft");
				else
					ReleaseKey("Common.StrafeLeft");
				//PPather.WriteLine("StrageLeft: " + strafeLeft);
			}
			if (old_strafeRight != strafeRight)
			{
				KeyT.Wait();
				KeyT.Reset();
				if (strafeRight)
					PressKey("Common.StrafeRight");
				else
					ReleaseKey("Common.StrafeRight");
				//PPather.WriteLine("StrageRight: " + strafeRight);
			}

			/*
							if(rotateRight || rotateLeft)
							{
								double head = GContext.Main.Me.Heading;
								if(rotateRight) head -= Math.PI/2;
								else if(rotateLeft)  head += Math.PI/2;
								GContext.Main.StartSpinTowards(head);
							}
							else
							{
								GContext.Main.ReleaseSpin();
							}
			*/

			if (old_rotateLeft != rotateLeft)
			{
				KeyT.Wait();
				KeyT.Reset();
				if (rotateLeft)
					PressKey("Common.RotateLeft");
				else
					ReleaseKey("Common.RotateLeft");
				//PPather.WriteLine("RotateLeft: " + rotateLeft);
			}
			if (old_rotateRight != rotateRight)
			{
				KeyT.Wait();
				KeyT.Reset();
				if (rotateRight)
					PressKey("Common.RotateRight");
				else
					ReleaseKey("Common.RotateRight");
				//PPather.WriteLine("RotateRight: " + rotateRight);
			}

			old_runForwards = runForwards;
			old_runBackwards = runBackwards;
			old_strafeLeft = strafeLeft;
			old_strafeRight = strafeRight;
			old_rotateLeft = rotateLeft;
			old_rotateRight = rotateRight;
		}

		void PressKey(string name)
		{
			//PPather.WriteLine("Press : " + name);
			Context.PressKey(name);
		}

		void ReleaseKey(string name)
		{
			//PPather.WriteLine("Release: " + name);
			Context.ReleaseKey(name);
		}

		void SendKey(string name)
		{
			//PPather.WriteLine("Send: " + name);
			Context.SendKey(name);
		}


		public void Jump()
		{
			SendKey("Common.Jump");
		}

		public void SwimUp(bool go)
		{
			if (go)
				PressKey("Common.Jump");
			else
				ReleaseKey("Common.Jump");
		}

		public void ResyncKeys()
		{
			KeyT.ForceReady();
			PushKeys();
		}

		public void MoveRandom()
		{
			int d = PPather.random.Next(4);
			if (d == 0)
				Forwards(true);
			if (d == 1)
				StrafeRight(true);
			if (d == 2)
				Backwards(true);
			if (d == 3)
				StrafeLeft(true);
		}

		public void StrafeLeft(bool go)
		{
			strafeLeft = go;
			if (go)
				strafeRight = false;
			PushKeys();
		}

		public void StrafeRight(bool go)
		{
			strafeRight = go;
			if (go)
				strafeLeft = false;
			PushKeys();
		}

		public void RotateLeft(bool go)
		{
			rotateLeft = go;
			if (go)
				rotateRight = false;
			PushKeys();
		}


		public void RotateRight(bool go)
		{
			rotateRight = go;
			if (go)
				rotateLeft = false;
			PushKeys();
		}


		public void Forwards(bool go)
		{
			runForwards = go;
			if (go)
				runBackwards = false;
			PushKeys();
		}

		public void Backwards(bool go)
		{
			runBackwards = go;
			if (go)
				runForwards = false;
			PushKeys();
		}

		public void StopRotate()
		{
			rotateLeft = false;
			rotateRight = false;
			PushKeys();
		}

		public void StopMove()
		{
			runForwards = false;
			runBackwards = false;
			strafeLeft = false;
			strafeRight = false;
			rotateLeft = false;
			rotateRight = false;
			SwimUp(false);
			PushKeys();
		}


		public void Stop()
		{
			StopMove();
			StopRotate();
		}

		public bool IsMoving()
		{
			return runForwards || runBackwards || strafeLeft || strafeRight;
		}


		public bool IsRotating()
		{
			return rotateLeft || rotateRight;
		}

		public bool IsRotatingLeft()
		{
			return rotateLeft;
		}
		public bool IsRotatingRight()
		{
			return rotateLeft;
		}


		/*
		  1 - location is front
		  2 - location is right
		  3 - location is back
		  4 - location is left
		*/
		int GetLocationDirection(GLocation loc)
		{
			int dir = 0;
			double b = loc.Bearing;
			if (b > -PI / 4 && b <= PI / 4)  // Front
			{
				dir = 1;
			}
			if (b > -3 * PI / 4 && b <= -PI / 4) // Left
			{
				dir = 4;
			}
			if (b <= -3 * PI / 4 || b > 3 * PI / 4) //  Back   
			{
				dir = 3;
			}
			if (b > PI / 4 && b <= 3 * PI / 4) //  Right  
			{
				dir = 2;
			}
			if (dir == 0)
				PPather.WriteLine("Odd, no known direction");

			return dir;
		}

		public static double GetDistance3D(GLocation l0, GLocation l1)
		{
			double dx = l0.X - l1.X;
			double dy = l0.Y - l1.Y;
			double dz = l0.Z - l1.Z;
			return Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		public bool moveTowardsFacing(GPlayerSelf Me, GLocation to, double distance, GLocation facing)
		{
			bool moving = false;
			double d = Me.Location.GetDistanceTo(to);
			if (d > distance)
			{
				int dir = GetLocationDirection(to);
				if (dir != 0)
					moving |= true;
				if (dir == 1 || dir == 3 || dir == 0)
				{
					StrafeLeft(false);
					StrafeRight(false);
				};
				if (dir == 2 || dir == 4 || dir == 0)
				{
					Forwards(false);
					Backwards(false);
				};
				if (dir == 1)
					Forwards(true);
				if (dir == 2)
					StrafeRight(true);
				if (dir == 3)
					Backwards(true);
				if (dir == 4)
					StrafeLeft(true);
				//PPather.WriteLine("Move dir: " + dir);
			}
			else
			{
				//PPather.WriteLine("Move is close");
				StrafeLeft(false);
				StrafeRight(false);
				Forwards(false);
				Backwards(false);
			}
			double bearing = Me.GetHeadingDelta(facing);
			if (bearing < -PI / 8)
			{
				moving |= true;
				RotateLeft(true);
			}
			else if (bearing > PI / 8)
			{
				moving |= true;
				RotateRight(true);
			}
			else
				StopRotate();

			return moving;
		}

		public double GetMoveHeading(out double speed)
		{
			double head = GContext.Main.Me.Heading;
			double r = 0;
			;
			speed = 0.0;
			if (runForwards)
			{
				speed = 7.0;
				r = head;
				if (strafeRight)
					r += PI / 2;
				if (strafeLeft)
					r -= PI / 2;
				if (runBackwards)
					speed = 0.0;
			}
			else if (runBackwards)
			{
				speed = 4.5;
				r = head + PI;
				if (strafeRight)
					r -= PI / 2;
				if (strafeLeft)
					r += PI / 2;
				if (runBackwards)
					speed = 0.0;
			}
			else if (strafeLeft)
			{
				speed = 7.0;
				r = head + PI * 3.0 / 4.0;
				if (strafeRight)
					speed = 0;
			}
			else if (strafeRight)
			{
				speed = 7.0;
				r = head + PI / 4;
			}

			if (head >= 2 * PI)
				head -= 2 * PI;
			return head;
		}
	}

	public class StuckDetecter
	{
		GLocation oldLocation = null;
		float predictedDX;
		float predictedDY;

		GSpellTimer StuckTimeout = new GSpellTimer(333); // Check every 333ms
		GPlayerSelf Me;
		GContext Context;
		Mover mover;
		int stuckSensitivity;
		int abortSensitivity;
		int stuckMove = 0;
		GSpellTimer lastStuckCheck = new GSpellTimer(0);
		bool firstStuckCheck = true;
		//GLocation StuckLocation = null;

		public StuckDetecter(PPather pather, int stuckSensitivity, int abortSensitivity)
		{
			this.Me = GContext.Main.Me;
			this.Context = GContext.Main;
			this.stuckSensitivity = stuckSensitivity;
			this.abortSensitivity = abortSensitivity;
			this.mover = PPather.mover;
			firstStuckCheck = true;
		}

		public bool checkStuck()
		{
			if (firstStuckCheck)
			{
				oldLocation = GContext.Main.Me.Location;
				predictedDX = 0;
				predictedDY = 0;
				firstStuckCheck = false;
				lastStuckCheck.Reset();
			}
			else
			{
				// update predicted location
				double h;
				double speed;
				h = mover.GetMoveHeading(out speed);

				float dt = (float)-lastStuckCheck.TicksLeft / 1000f;
				float dx = (float)Math.Cos(h) * (float)speed * dt;
				float dy = (float)Math.Sin(h) * (float)speed * dt;
				//PPather.WriteLine("speed: " + speed + " dt: " + dt + " dx: " + dx + " dy : " + dy);
				predictedDX += dx;
				predictedDY += dy;

				lastStuckCheck.Reset();
				if (StuckTimeout.IsReady)
				{
					// Check stuck
					GLocation loc = Me.Location;
					float realDX = loc.X - oldLocation.X;
					float realDY = loc.Y - oldLocation.Y;
					//PPather.WriteLine(" dx: " + predictedDX + " dy : " + predictedDY + " Real dx: " + realDX + " dy : " + realDY );

					float predictDist = (float)Math.Sqrt(predictedDX * predictedDX + predictedDY * predictedDY);
					float realDist = (float)Math.Sqrt(realDX * realDX + realDY * realDY);

					//PPather.WriteLine(" pd " + predictDist + " rd " + realDist);

					int multiplier = 3;
					if (GPlayerSelf.Me.IsStealth)
						multiplier = 4;

					if (predictDist > realDist * multiplier)
					{
						// moving a lot slower than predicted
						// check direction
						GLocation excpected = new GLocation(loc.X + predictedDX, loc.Y + predictedDY);

						PPather.WriteLine("I am stuck " + stuckMove); //. Jumping to get free");
						if (stuckMove == 0)
						{
							mover.Forwards(false);
							mover.Forwards(true);
							mover.StrafeLeft(true);
							//mover.Jump();
							//mover.StrafeRight(false);
						}
						else if (stuckMove == 1)
						{
							mover.Forwards(false);
							mover.Forwards(true);
							mover.StrafeLeft(true);
							//PPather.WriteLine("  strafe left"); 
							//mover.Jump();
							//mover.StrafeLeft(false);
						}
						else if (stuckMove == 2)
						{
							mover.Forwards(false);
							mover.Forwards(true);
							mover.StrafeRight(true);
							//PPather.WriteLine("  strafe left"); 
							//mover.StrafeLeft(true);
						}
						else if (stuckMove == 2)
						{
							mover.Forwards(false);
							mover.Forwards(true);
							mover.StrafeRight(true);
							//PPather.WriteLine("  strafe left"); 
							//mover.StrafeLeft(true);
						}
						stuckMove++;
						if (stuckMove >= abortSensitivity)
						{
							return true;
						}

					}
					else
					{
						stuckMove = 0;

					}
					predictedDX = 0;
					predictedDY = 0;
					oldLocation = loc;
					StuckTimeout.Reset();
				}


			}
			return false;
		}

	}

	public class EasyMover
	{
		Location target;
		GPlayerSelf Me;
		GContext Context;
		PathGraph world = null;

		Mover mover;
		PPather pather;
		bool GiveUpIfStuck = false;
		bool GiveUpIfUnsafe = false;

		GSpellTimer PathTimeout = new GSpellTimer(5000); // no path can be older than this
		MoveAlonger MoveAlong;

		public enum MoveResult
		{
			Reached,
			Stuck,
			CantFindPath,
			Unsafe,
			Moving,
			GotThere
		};

		public EasyMover(PPather pather, Location target, bool GiveUpIfStuck,
						bool GiveUpIfUnsafe)
		{
			this.target = target;
			this.Me = GContext.Main.Me;
			this.Context = GContext.Main;
			this.world = PPather.world;
			mover = PPather.mover;
			this.GiveUpIfStuck = GiveUpIfStuck;
			this.GiveUpIfUnsafe = GiveUpIfUnsafe;
			this.pather = pather;
		}

		public void SetPathTimeout(int ms)
		{
			PathTimeout = new GSpellTimer(ms); // no path can be older than this                
		}

		public void SetNewTarget(Location target)
		{
			MoveAlong = null;
			this.target = target;
		}

		public MoveResult move()
		{
			return move(Context.MeleeDistance);
		}

		public MoveResult move(double howClose)
		{
			if (GiveUpIfUnsafe)
			{
				if (!pather.IsItSafeAt(Me.Target, Me.Location))
					return MoveResult.Unsafe;
			}
			if (PathTimeout.IsReady)
			{
				MoveAlong = null;
			}
			if (MoveAlong == null)
			{
				Location from = new Location(Me.Location);
				mover.Stop();
				Path path = world.CreatePath(from, target, (float)howClose, PPather.radar);
				PathTimeout.Reset();
				if (path == null || path.Count() == 0)
				{
					PPather.WriteLine("EasyMover: Can not create path . giving up");
					mover.MoveRandom();
					Thread.Sleep(200);
					mover.Stop();
					return MoveResult.CantFindPath;
				}
				else
				{
					//PPather.WriteLine("Save path to " + pathfile); 
					//path.Save(pathfile);
					MoveAlong = new MoveAlonger(pather, path);
				}
			}

			if (MoveAlong != null)
			{
				Location from = new Location(Me.Location);

				if (MoveAlong.path.Count() == 0 || from.GetDistanceTo(target) < howClose)
				{
					//PPather.WriteLine("Move along used up");
					MoveAlong = null;
					mover.Stop();
					return MoveResult.GotThere;
				}
				else if (!MoveAlong.MoveAlong())
				{
					MoveAlong = null; // got stuck!
					if (GiveUpIfStuck)
						return MoveResult.Stuck;
				}
			}
			return MoveResult.Moving;
		}
	}

	public class MoveAlonger
	{
		public Path path;
		StuckDetecter sd;
		GPlayerSelf Me;
		GContext Context;
		Location prev;
		Location current;
		Location next;
		Mover mover;
		PathGraph world = null;
		int blockCount = 0;

		public MoveAlonger(PPather pather, Path path)
		{
			this.Context = GContext.Main;
			this.Me = Context.Me;
			this.path = path;
			this.world = PPather.world;
			mover = PPather.mover;
			sd = new StuckDetecter(pather, 1, 2);
			prev = null;
			current = path.GetFirst();
			next = path.GetSecond();
		}

		public bool MoveAlong()
		{
			double max = 3.0;
			GLocation loc = GContext.Main.Me.Location;
			Location isAt = new Location(loc.X, loc.Y, loc.Z);
			/*
			while (isAt.GetDistanceTo(current) < max && next != null)
			{
				//PPather.WriteLine(current + " - " + next);
				path.RemoveFirst();
				if (path.Count() == 0)
				{
					//PPather.WriteLine("ya");
					//return true; // good in some way
				}
				else
				{
					prev = current;
					current = path.GetFirst();
					next = path.GetSecond();
				}
			}
*/
			bool consume = false;
			do
			{

				bool blocked = false;

				consume = false;
				if (next != null)
					world.triangleWorld.IsStepBlocked(loc.X, loc.Y, loc.Z, next.X, next.Y, next.Z,
													  PathGraph.toonHeight, PathGraph.toonSize, null);
				double d = isAt.GetDistanceTo(current);
				if ((d < max && !blocked) ||
				   d < 1.5)
					consume = true;

				if (consume)
				{
					//PPather.WriteLine("Consume spot " + current + " d " + d + " block " + blocked);
					path.RemoveFirst();
					if (path.Count() == 0)
					{
						break;
					}
					else
					{
						prev = current;
						current = path.GetFirst();
						next = path.GetSecond();
					}
				}

			} while (consume);

			{
				//PPather.WriteLine("Move towards " + current);
				GLocation gto = new GLocation((float)current.X, (float)current.Y, (float)current.Z);
				GLocation face;
				if (next != null)
					face = new GLocation(next.X, next.Y, next.Z);
				else
					face = gto;

				if (!mover.moveTowardsFacing(Me, gto, 0.5, face))
				{
					PPather.WriteLine("Can't move " + current);
					world.BlacklistStep(prev, current);
					//world.MarkStuckAt(loc, Me.Heading);                        
					mover.MoveRandom();
					Thread.Sleep(500);
					mover.Stop();
					return false;
					// hmm, mover does not want to move, must be up or down
				}

				{
					double h;
					double speed;
					h = mover.GetMoveHeading(out speed);
					float stand_z = 0.0f;
					int flags = 0;
					float x = isAt.X + (float)Math.Cos(h) * 1.0f;
					float y = isAt.Y + (float)Math.Sin(h) * 1.0f;
					float z = isAt.Z;
					bool aheadOk = world.triangleWorld.FindStandableAt(x, y,
																	   z - 4,
																	   z + 6,
																	   out stand_z, out flags, 0, 0);
					if (!aheadOk)
					{
						blockCount++;
						PPather.WriteLine("Heading into a wall or off a cliff " + blockCount);
						world.MarkStuckAt(isAt, (float)Me.Heading);

						if (prev != null)
						{
							GLocation gprev = new GLocation((float)prev.X, (float)prev.Y, (float)prev.Z);

							if (!mover.moveTowardsFacing(Me, gprev, 0.5, face))
							{
								mover.Stop();
								return false;
							}
						}
						if (blockCount > 1)
						{
							world.BlacklistStep(prev, current);
							return false;
						}

						return true;
					}
					else
						blockCount = 0;
				}


				if (sd.checkStuck())
				{
					PPather.WriteLine("Stuck at " + isAt);
					world.MarkStuckAt(isAt, (float)Me.Heading);
					world.BlacklistStep(prev, current);
					mover.Stop();
					return false;
				}
			}
			return true;
		}
	}
}
