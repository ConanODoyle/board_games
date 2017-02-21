 function player::makeGhost(%pl)
{
        %pl.startFade(0,0,1);
        %p = 0.5;
        %a = -1;
        %node[%a++] = "headskin";
        %node[%a++] = "chest";
        %node[%a++] = "femChest";
        %node[%a++] = "pants";
        %node[%a++] = "lpeg";
        %node[%a++] = "rpeg";
        %node[%a++] = "lshoe";
        %node[%a++] = "rshoe";
        %node[%a++] = "rarm";
        %node[%a++] = "larm";
        for(%i=0;$hat[%i]!$="";%i++)
                %node[%a++] = $hat[%i];
        for(%i=0;$pack[%i]!$="";%i++)
                %node[%a++] = $pack[%i];
        for(%i=0;$secondPack[%i]!$="";%i++)
                %node[%a++] = $secondPack[%i];
        for(%i=0;$accent[%i]!$="";%i++)
                %node[%a++] = $accent[%i];
        %node[%a++] = "skirthip";
        %node[%a++] = "skirttrimleft";
        %node[%a++] = "skirttrimright";
        %node[%a++] = "rarmslim";
        %node[%a++] = "larmslim";
        %node[%a++] = "lhand";
        %node[%a++] = "rhand";
        %node[%a++] = "lhook";
        %node[%a++] = "rhook";
        
        for(%i=0;%i<=%a;%i++)
        {
                if(%pl.isNodeVisible(%node[%i]))
                        %pl.setNodeColor(%node[%i],getWords(%pl.nodeColor[%node[%i]],0,2) SPC (mCos((%p)*($PI))+1)/2);
        }
}
package swol_ghost
{
        function player::setNodeColor(%pl,%node,%col)
        {
                %pl.nodeColor[%node] = %col;
                parent::setNodeColor(%pl,%node,%col);
        }
};
activatePackage(swol_ghost);