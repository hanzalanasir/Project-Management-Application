import{B as KE,Ct as TE,D as HD,E as Gv,Gr as zE,Kn as n1,Mr as wE,Mt as Up,Nr as wi,Or as vD,Rn as kn,Sr as tu,St as T,Vt as YE,Xt as Zp,ar as qe,dn as e1,dr as rD,dt as Q,ft as QE,i as A,j as Hp,kt as Uc,l as Bp,n as $p,nt as Nu,pt as Qc,qt as ZE,r as $u,u as Bu,un as dh,ur as r1,v as Er,wr as uD,xt as Su}from"./chunk-CXUl5D6b.js";import{D as kn$1,S as ar,d as St,t as Bt,y as Zn}from"./chunk-DUInHNO1.js";import{B as W,D as re,k as I,s as We}from"./main-TOWMQA7S.js";import{n as Nt,t as Lt}from"./chunk-BVus6FZL.js";import{t as Ct}from"./chunk-Cl9gf3FL.js";var et=(()=>{class i{static ɵfac=function(a){return new(a||i)};static ɵmod=TE({type:i});static ɵinj=tu({imports:[kn$1,re,St,I]})}return i})();function at(i,m){if(i&1&&(wi(0,`mat-option`,17),HD(1),Uc()),i&2){let t=m.$implicit;$p(`value`,t),Gv(),Qc(` `,t,` `)}}function nt(i,m){if(i&1){let t=rD();wi(0,`mat-form-field`,14)(1,`mat-select`,16,0),Zp(`selectionChange`,function(e){Nu(t);return Su(uD(2)._changePageSize(e.value))}),YE(3,at,2,2,`mat-option`,17,ZE),Uc(),wi(5,`div`,18),Zp(`click`,function(){Nu(t);return Su(vD(2).open())}),Uc()()}if(i&2){let t=uD(2);$p(`appearance`,t._formFieldAppearance)(`color`,t.color),Gv(),$p(`value`,t.pageSize)(`disabled`,t.disabled),Hp(`aria-labelledby`,t._pageSizeLabelId),$p(`panelClass`,t.selectConfig.panelClass||``)(`disableOptionCentering`,t.selectConfig.disableOptionCentering),Gv(2),KE(t._displayedPageSizeOptions)}}function ot(i,m){if(i&1&&(wi(0,`div`,15),HD(1),Uc()),i&2){let t=uD(2);Gv(),dh(t.pageSize)}}function rt(i,m){if(i&1&&(wi(0,`div`,3)(1,`div`,13),HD(2),Uc(),zE(3,nt,6,7,`mat-form-field`,14),zE(4,ot,2,1,`div`,15),Uc()),i&2){let t=uD();Gv(),Bp(`id`,t._pageSizeLabelId),Gv(),Qc(` `,t._intl.itemsPerPageLabel,` `),Gv(),QE(t._displayedPageSizeOptions.length>1?3:-1),Gv(),QE(t._displayedPageSizeOptions.length<=1?4:-1)}}function st(i,m){if(i&1){let t=rD();wi(0,`button`,19),Zp(`click`,function(){Nu(t);let e=uD();return Su(e._buttonClicked(0,e._previousButtonsDisabled()))}),Bu(),wi(1,`svg`,8),Up(2,`path`,20),Uc()()}if(i&2){let t=uD();$p(`matTooltip`,t._intl.firstPageLabel)(`matTooltipDisabled`,t._previousButtonsDisabled())(`disabled`,t._previousButtonsDisabled())(`tabindex`,t._previousButtonsDisabled()?-1:null),Bp(`aria-label`,t._intl.firstPageLabel)}}function lt(i,m){if(i&1){let t=rD();wi(0,`button`,21),Zp(`click`,function(){Nu(t);let e=uD();return Su(e._buttonClicked(e.getNumberOfPages()-1,e._nextButtonsDisabled()))}),Bu(),wi(1,`svg`,8),Up(2,`path`,22),Uc()()}if(i&2){let t=uD();$p(`matTooltip`,t._intl.lastPageLabel)(`matTooltipDisabled`,t._nextButtonsDisabled())(`disabled`,t._nextButtonsDisabled())(`tabindex`,t._nextButtonsDisabled()?-1:null),Bp(`aria-label`,t._intl.lastPageLabel)}}var pt=(()=>{class i{changes=new Q;itemsPerPageLabel=`Items per page:`;nextPageLabel=`Next page`;previousPageLabel=`Previous page`;firstPageLabel=`First page`;lastPageLabel=`Last page`;getRangeLabel=(t,a,e)=>{if(e==0||a==0)return`0 of ${e}`;e=Math.max(e,0);let s=t*a,P=s<e?Math.min(s+a,e):s+a;return`${s+1} \u2013 ${P} of ${e}`};static ɵfac=function(a){return new(a||i)};static ɵprov=Er({token:i,factory:i.ɵfac})}return i})();var dt=50;var mt=new A(`MAT_PAGINATOR_DEFAULT_OPTIONS`);var gt=(()=>{class i{_intl=T(pt);_changeDetectorRef=T(e1);_formFieldAppearance;_pageSizeLabelId=T(Bt).getId(`mat-paginator-page-size-label-`);_intlChanges;_isInitialized=!1;_initializedStream=new kn(1);color;get pageIndex(){return this._pageIndex}set pageIndex(t){this._pageIndex=Math.max(t||0,0),this._changeDetectorRef.markForCheck()}_pageIndex=0;get length(){return this._length}set length(t){this._length=t||0,this._changeDetectorRef.markForCheck()}_length=0;get pageSize(){return this._pageSize}set pageSize(t){this._pageSize=Math.max(t||0,0),this._updateDisplayedPageSizeOptions()}_pageSize;get pageSizeOptions(){return this._pageSizeOptions}set pageSizeOptions(t){this._pageSizeOptions=(t||[]).map(a=>r1(a,0)),this._updateDisplayedPageSizeOptions()}_pageSizeOptions=[];hidePageSize=!1;showFirstLastButtons=!1;selectConfig={};disabled=!1;page=new qe;_displayedPageSizeOptions;initialized=this._initializedStream;constructor(){let t=this._intl,a=T(mt,{optional:!0});if(this._intlChanges=t.changes.subscribe(()=>this._changeDetectorRef.markForCheck()),a){let{pageSize:e,pageSizeOptions:s,hidePageSize:P,showFirstLastButtons:M}=a;e!=null&&(this._pageSize=e),s!=null&&(this._pageSizeOptions=s),P!=null&&(this.hidePageSize=P),M!=null&&(this.showFirstLastButtons=M)}this._formFieldAppearance=a?.formFieldAppearance||`outline`}ngOnInit(){this._isInitialized=!0,this._updateDisplayedPageSizeOptions(),this._initializedStream.next()}ngOnDestroy(){this._initializedStream.complete(),this._intlChanges.unsubscribe()}nextPage(){this.hasNextPage()&&this._navigate(this.pageIndex+1)}previousPage(){this.hasPreviousPage()&&this._navigate(this.pageIndex-1)}firstPage(){this.hasPreviousPage()&&this._navigate(0)}lastPage(){this.hasNextPage()&&this._navigate(this.getNumberOfPages()-1)}hasPreviousPage(){return this.pageIndex>=1&&this.pageSize!=0}hasNextPage(){let t=this.getNumberOfPages()-1;return this.pageIndex<t&&this.pageSize!=0}getNumberOfPages(){return this.pageSize?Math.ceil(this.length/this.pageSize):0}_changePageSize(t){let a=this.pageIndex*this.pageSize,e=this.pageIndex;this.pageIndex=Math.floor(a/t)||0,this.pageSize=t,this._emitPageEvent(e)}_nextButtonsDisabled(){return this.disabled||!this.hasNextPage()}_previousButtonsDisabled(){return this.disabled||!this.hasPreviousPage()}_updateDisplayedPageSizeOptions(){this._isInitialized&&(this.pageSize||(this._pageSize=this.pageSizeOptions.length!=0?this.pageSizeOptions[0]:dt),this._displayedPageSizeOptions=this.pageSizeOptions.slice(),this._displayedPageSizeOptions.indexOf(this.pageSize)===-1&&this._displayedPageSizeOptions.push(this.pageSize),this._displayedPageSizeOptions.sort((t,a)=>t-a),this._changeDetectorRef.markForCheck())}_emitPageEvent(t){this.page.emit({previousPageIndex:t,pageIndex:this.pageIndex,pageSize:this.pageSize,length:this.length})}_navigate(t){let a=this.pageIndex;t!==a&&(this.pageIndex=t,this._emitPageEvent(a))}_buttonClicked(t,a){a||this._navigate(t)}static ɵfac=function(a){return new(a||i)};static ɵcmp=wE({type:i,selectors:[[`mat-paginator`]],hostAttrs:[`role`,`group`,1,`mat-mdc-paginator`],inputs:{color:`color`,pageIndex:[2,`pageIndex`,`pageIndex`,r1],length:[2,`length`,`length`,r1],pageSize:[2,`pageSize`,`pageSize`,r1],pageSizeOptions:`pageSizeOptions`,hidePageSize:[2,`hidePageSize`,`hidePageSize`,n1],showFirstLastButtons:[2,`showFirstLastButtons`,`showFirstLastButtons`,n1],selectConfig:`selectConfig`,disabled:[2,`disabled`,`disabled`,n1]},outputs:{page:`page`},exportAs:[`matPaginator`],decls:14,vars:14,consts:[[`selectRef`,``],[1,`mat-mdc-paginator-outer-container`],[1,`mat-mdc-paginator-container`],[1,`mat-mdc-paginator-page-size`],[1,`mat-mdc-paginator-range-actions`],[`aria-atomic`,`true`,`aria-live`,`polite`,`role`,`status`,1,`mat-mdc-paginator-range-label`],[`matIconButton`,``,`type`,`button`,`matTooltipPosition`,`above`,`disabledInteractive`,``,1,`mat-mdc-paginator-navigation-first`,3,`matTooltip`,`matTooltipDisabled`,`disabled`,`tabindex`],[`matIconButton`,``,`type`,`button`,`matTooltipPosition`,`above`,`disabledInteractive`,``,1,`mat-mdc-paginator-navigation-previous`,3,`click`,`matTooltip`,`matTooltipDisabled`,`disabled`,`tabindex`],[`viewBox`,`0 0 24 24`,`focusable`,`false`,`aria-hidden`,`true`,1,`mat-mdc-paginator-icon`],[`d`,`M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z`],[`matIconButton`,``,`type`,`button`,`matTooltipPosition`,`above`,`disabledInteractive`,``,1,`mat-mdc-paginator-navigation-next`,3,`click`,`matTooltip`,`matTooltipDisabled`,`disabled`,`tabindex`],[`d`,`M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z`],[`matIconButton`,``,`type`,`button`,`matTooltipPosition`,`above`,`disabledInteractive`,``,1,`mat-mdc-paginator-navigation-last`,3,`matTooltip`,`matTooltipDisabled`,`disabled`,`tabindex`],[`aria-hidden`,`true`,1,`mat-mdc-paginator-page-size-label`],[1,`mat-mdc-paginator-page-size-select`,3,`appearance`,`color`],[1,`mat-mdc-paginator-page-size-value`],[`hideSingleSelectionIndicator`,``,3,`selectionChange`,`value`,`disabled`,`aria-labelledby`,`panelClass`,`disableOptionCentering`],[3,`value`],[1,`mat-mdc-paginator-touch-target`,3,`click`],[`matIconButton`,``,`type`,`button`,`matTooltipPosition`,`above`,`disabledInteractive`,``,1,`mat-mdc-paginator-navigation-first`,3,`click`,`matTooltip`,`matTooltipDisabled`,`disabled`,`tabindex`],[`d`,`M18.41 16.59L13.82 12l4.59-4.59L17 6l-6 6 6 6zM6 6h2v12H6z`],[`matIconButton`,``,`type`,`button`,`matTooltipPosition`,`above`,`disabledInteractive`,``,1,`mat-mdc-paginator-navigation-last`,3,`click`,`matTooltip`,`matTooltipDisabled`,`disabled`,`tabindex`],[`d`,`M5.59 7.41L10.18 12l-4.59 4.59L7 18l6-6-6-6zM16 6h2v12h-2z`]],template:function(a,e){a&1&&(wi(0,`div`,1)(1,`div`,2),zE(2,rt,5,4,`div`,3),wi(3,`div`,4)(4,`div`,5),HD(5),Uc(),zE(6,st,3,5,`button`,6),wi(7,`button`,7),Zp(`click`,function(){return e._buttonClicked(e.pageIndex-1,e._previousButtonsDisabled())}),Bu(),wi(8,`svg`,8),Up(9,`path`,9),Uc()(),$u(),wi(10,`button`,10),Zp(`click`,function(){return e._buttonClicked(e.pageIndex+1,e._nextButtonsDisabled())}),Bu(),wi(11,`svg`,8),Up(12,`path`,11),Uc()(),zE(13,lt,3,5,`button`,12),Uc()()()),a&2&&(Gv(2),QE(e.hidePageSize?-1:2),Gv(3),Qc(` `,e._intl.getRangeLabel(e.pageIndex,e.pageSize,e.length),` `),Gv(),QE(e.showFirstLastButtons?6:-1),Gv(),$p(`matTooltip`,e._intl.previousPageLabel)(`matTooltipDisabled`,e._previousButtonsDisabled())(`disabled`,e._previousButtonsDisabled())(`tabindex`,e._previousButtonsDisabled()?-1:null),Bp(`aria-label`,e._intl.previousPageLabel),Gv(3),$p(`matTooltip`,e._intl.nextPageLabel)(`matTooltipDisabled`,e._nextButtonsDisabled())(`disabled`,e._nextButtonsDisabled())(`tabindex`,e._nextButtonsDisabled()?-1:null),Bp(`aria-label`,e._intl.nextPageLabel),Gv(3),QE(e.showFirstLastButtons?13:-1))},dependencies:[We,Nt,W,Zn,Ct],styles:[`.mat-mdc-paginator {
  display: block;
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  color: var(--%NS%mat-paginator-container-text-color, var(--%NS%mat-sys-on-surface));
  background-color: var(--%NS%mat-paginator-container-background-color, var(--%NS%mat-sys-surface));
  font-family: var(--%NS%mat-paginator-container-text-font, var(--%NS%mat-sys-body-small-font));
  line-height: var(--%NS%mat-paginator-container-text-line-height, var(--%NS%mat-sys-body-small-line-height));
  font-size: var(--%NS%mat-paginator-container-text-size, var(--%NS%mat-sys-body-small-size));
  font-weight: var(--%NS%mat-paginator-container-text-weight, var(--%NS%mat-sys-body-small-weight));
  letter-spacing: var(--%NS%mat-paginator-container-text-tracking, var(--%NS%mat-sys-body-small-tracking));
  --%NS%mat-form-field-container-height: var(--%NS%mat-paginator-form-field-container-height, 40px);
  --%NS%mat-form-field-container-vertical-padding: var(--%NS%mat-paginator-form-field-container-vertical-padding, 8px);
}
.mat-mdc-paginator .mat-mdc-select-value {
  font-size: var(--%NS%mat-paginator-select-trigger-text-size, var(--%NS%mat-sys-body-small-size));
}
.mat-mdc-paginator .mat-mdc-form-field-subscript-wrapper {
  display: none;
}
.mat-mdc-paginator .mat-mdc-select {
  line-height: 1.5;
}

.mat-mdc-paginator-outer-container {
  display: flex;
}

.mat-mdc-paginator-container {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  padding: 0 8px;
  flex-wrap: wrap;
  width: 100%;
  min-height: var(--%NS%mat-paginator-container-size, 56px);
}

.mat-mdc-paginator-page-size {
  display: flex;
  align-items: baseline;
  margin-right: 8px;
}
[dir=rtl] .mat-mdc-paginator-page-size {
  margin-right: 0;
  margin-left: 8px;
}

.mat-mdc-paginator-page-size-label {
  margin: 0 4px;
}

.mat-mdc-paginator-page-size-select {
  margin: 0 4px;
  width: var(--%NS%mat-paginator-page-size-select-width, 84px);
}

.mat-mdc-paginator-range-label {
  margin: 0 32px 0 24px;
}

.mat-mdc-paginator-range-actions {
  display: flex;
  align-items: center;
}

.mat-mdc-paginator-icon {
  display: inline-block;
  width: 28px;
  fill: var(--%NS%mat-paginator-enabled-icon-color, var(--%NS%mat-sys-on-surface-variant));
}
.mat-mdc-icon-button[aria-disabled] .mat-mdc-paginator-icon {
  fill: var(--%NS%mat-paginator-disabled-icon-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
[dir=rtl] .mat-mdc-paginator-icon {
  transform: rotate(180deg);
}

@media (forced-colors: active) {
  .mat-mdc-icon-button[aria-disabled] .mat-mdc-paginator-icon,
  .mat-mdc-paginator-icon {
    fill: currentColor;
  }
  .mat-mdc-paginator-range-actions .mat-mdc-icon-button {
    outline: solid 1px;
  }
  .mat-mdc-paginator-range-actions .mat-mdc-icon-button[aria-disabled] {
    color: GrayText;
  }
}
.mat-mdc-paginator-touch-target {
  display: var(--%NS%mat-paginator-touch-target-display, block);
  position: absolute;
  top: 50%;
  left: 50%;
  width: var(--%NS%mat-paginator-page-size-select-width, 84px);
  height: var(--%NS%mat-paginator-page-size-select-touch-target-height, 48px);
  background-color: transparent;
  transform: translate(-50%, -50%);
  cursor: pointer;
}
`],encapsulation:2})}return i})();var kt=(()=>{class i{static ɵfac=function(a){return new(a||i)};static ɵmod=TE({type:i});static ɵinj=tu({imports:[ar,Lt,et,gt]})}return i})();export{kt as n,gt as t};