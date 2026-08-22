#!/usr/bin/env python3
from pathlib import Path
import math

OUT=Path('Assets/Fsp/Art/Resources/Models/BMG/Production')
OUT.mkdir(parents=True,exist_ok=True)

class O:
 def __init__(s,n): s.n=n;s.v=[];s.f=[]
 def V(s,p): s.v.append(p);return len(s.v)
 def T(s,a,b,c): s.f.append((a,b,c))
 def Q(s,a,b,c,d): s.T(a,b,c);s.T(a,c,d)
 def box(s,c,z):
  x,y,w=c;a,b,d=z;P=[(x-a/2,y-b/2,w-d/2),(x+a/2,y-b/2,w-d/2),(x+a/2,y+b/2,w-d/2),(x-a/2,y+b/2,w-d/2),(x-a/2,y-b/2,w+d/2),(x+a/2,y-b/2,w+d/2),(x+a/2,y+b/2,w+d/2),(x-a/2,y+b/2,w+d/2)];I=[s.V(p) for p in P]
  for A,B,C,D in [(0,1,2,3),(4,7,6,5),(0,4,5,1),(1,5,6,2),(2,6,7,3),(4,0,3,7)]:s.Q(I[A],I[B],I[C],I[D])
 def cyl(s,c,r,h,N=18,axis='y'):
  x,y,z=c;A=[];B=[]
  for i in range(N):
   q=2*math.pi*i/N;u,v=r*math.cos(q),r*math.sin(q)
   if axis=='y': A.append(s.V((x+u,y-h/2,z+v)));B.append(s.V((x+u,y+h/2,z+v)))
   elif axis=='z': A.append(s.V((x+u,y+v,z-h/2)));B.append(s.V((x+u,y+v,z+h/2)))
   else:A.append(s.V((x-h/2,y+u,z+v)));B.append(s.V((x+h/2,y+u,z+v)))
  for i in range(N):j=(i+1)%N;s.Q(A[i],A[j],B[j],B[i])
 def sph(s,c,r,N=18,R=9,scale=(1,1,1)):
  x,y,z=c;rows=[]
  for j in range(1,R):
   p=math.pi*j/R;row=[]
   for i in range(N):
    t=2*math.pi*i/N;row.append(s.V((x+r*math.sin(p)*math.cos(t)*scale[0],y+r*math.cos(p)*scale[1],z+r*math.sin(p)*math.sin(t)*scale[2])))
   rows.append(row)
  top=s.V((x,y+r*scale[1],z));bot=s.V((x,y-r*scale[1],z))
  for i in range(N):j=(i+1)%N;s.T(top,rows[0][i],rows[0][j]);s.T(bot,rows[-1][j],rows[-1][i])
  for k in range(len(rows)-1):
   for i in range(N):j=(i+1)%N;s.Q(rows[k][i],rows[k+1][i],rows[k+1][j],rows[k][j])
 def save(s,name):
  p=OUT/name
  with p.open('w',newline='\n') as f:
   f.write('o '+s.n+'\ns 1\n')
   for v in s.v:f.write('v %.5f %.5f %.5f\n'%v)
   for q in s.f:f.write('f '+' '.join(map(str,q))+'\n')

def char(i,f=False):
 o=O('bmg_character_%02d'%i);h=.38 if f else .43
 for a in (-1,1):o.cyl((a*h/2,.68,0),.13,1.05);o.cyl((a*h/2,.14,.08),.15,.30,18,'z')
 o.sph((0,1.36,0),.55,20,10,(.80 if f else .92,1.15,.48));o.box((0,1.38,.02),(.78 if f else .92,.62,.36))
 for a in (-1,1):o.cyl((a*.52,1.38,0),.105,.90,16);o.sph((a*.52,.91,.02),.12,14,7)
 o.sph((0,2.07,0),.30,20,10,(.9,1.05,.9));o.sph((0,2.22,0),.33,20,9,(1,.55,1));o.box((0,1.42,-.34),(.60,.70,.22));o.box((0,1.09,.12),(.78,.14,.28))
 for x in (-.26,0,.26):o.box((x,1.04,.25),(.18,.22,.12))
 if i%2==0:o.cyl((0,2.02,.27),.12,.08,14,'z')
 if i%3==0:o.box((.42,1.22,.18),(.12,.32,.12))
 o.save('bmg_character_%02d.obj'%i)

def gun(name,smg=False):
 o=O(name);L=.72 if smg else 1.05;o.box((0,0,L*.05),(.18,.22,L*.52));o.box((0,.02,-L*.35),(.16,.18,L*.32));o.box((0,-.20,.03),(.13,.34,.18));o.cyl((0,0,L*.50),.045,L*.55,18,'z');o.cyl((0,0,L*.79),.065,.14,18,'z');o.box((0,.16,.05),(.09,.07,.22 if smg else .34));o.cyl((0,.22,.08),.075,.18,18,'z');o.box((0,-.16,L*.23),(.11,.30,.13));o.save(name+'.obj')

def buggy():
 o=O('bmg_buggy');o.box((0,.55,0),(1.9,.36,3));o.box((0,1.1,-.25),(1.55,.85,1.75))
 for x in (-.72,.72):
  for z in (-.72,.58):o.cyl((x,1.35,z),.045,1.35,12)
 for x in (-1,1):
  for z in (-1.05,1.05):o.cyl((x,.45,z),.38,.34,20,'x')
 o.box((0,.48,1.55),(1.75,.18,.18));o.box((0,.48,-1.55),(1.75,.18,.18));o.cyl((-.55,.78,1.48),.12,.08,16,'z');o.cyl((.55,.78,1.48),.12,.08,16,'z');o.save('bmg_buggy.obj')

def plane():
 o=O('bmg_transport_plane');o.cyl((0,0,0),.72,7.2,24,'z');o.sph((0,0,3.55),.78,24,10,(1,1,.8));o.sph((0,0,-3.65),.55,20,9,(1,1,1.5));o.box((0,-.02,.15),(9.5,.18,1.15));o.box((0,.15,-2.85),(3.4,.15,.72));o.box((0,.72,-3.15),(.16,1.55,.72))
 for x in (-2.5,2.5):o.cyl((x,-.3,.35),.38,1.55,20,'z');o.sph((x,-.3,1.1),.39,18,8,(1,1,.6))
 o.save('bmg_transport_plane.obj')

def chute():
 o=O('bmg_parachute');N=32;R=8;rows=[]
 for j in range(R+1):
  p=(math.pi/2)*j/R;r=2.25*math.sin(p);y=2.25*math.cos(p);rows.append([o.V((r*math.cos(2*math.pi*i/N),y,r*math.sin(2*math.pi*i/N)*.72)) for i in range(N)])
 for j in range(R):
  for i in range(N):k=(i+1)%N;o.Q(rows[j][i],rows[j+1][i],rows[j+1][k],rows[j][k])
 for x,z in [(-1.6,-.8),(1.6,-.8),(-1.6,.8),(1.6,.8)]:o.cyl((x*.38,-.65,z*.38),.018,2.15,8)
 o.save('bmg_parachute.obj')

def env():
 o=O('bmg_sunscar_environment');o.box((0,-1.5,10),(400,3,400));P=[(-70,20),(60,-40),(82,78),(-105,-63),(150,-120),(-10,138),(-132,86)]
 for q,(x,z) in enumerate(P):
  for b in range(5):bx=x+(b%3-1)*11;bz=z+(b//3)*12;o.box((bx,2.4,bz),(8+q%3,4.8,7+((b+q)%2)*2));o.box((bx,5,bz),(8.6+q%3,.35,7.6+((b+q)%2)*2))
  o.cyl((x+18,4,z+14),2.4,8,20)
 for i in range(48):a=2*math.pi*i/48;r=175+(i%5)*4;o.sph((math.cos(a)*r,2.2,math.sin(a)*r+8),5.5+i%4,14,7,(1.5,.65,1))
 for i in range(70):
  x=-170+(i*47)%340;z=-165+(i*71)%330
  if abs(x)<15 and abs(z)<15:continue
  o.cyl((x,1.6,z),.22,3.2,10);o.sph((x,4,z),1.4,12,6,(1,1.2,1))
 o.save('bmg_sunscar_environment.obj')

for i in range(1,7):char(i,i in (3,5))
gun('bmg_assault_rifle');gun('bmg_smg',True);buggy();plane();chute();env()
print('generated',len(list(OUT.glob('*.obj'))),'BMG production OBJ assets in',OUT)
