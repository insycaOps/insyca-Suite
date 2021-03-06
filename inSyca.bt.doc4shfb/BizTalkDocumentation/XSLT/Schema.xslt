<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:param name="GenDate" />
    <xsl:param name="DocVersion" />

    <xsl:output method="html" indent="no"/>

    <xsl:template match="/">
        <html>
            <HEAD>
                <link href="../CommentReport.css" type="text/css" rel="stylesheet"></link>
            </HEAD>

            <body>

                <xsl:call-template name="header" />

                <table class="TableData" border="0">
                    <tr>
                        <td width="10"></td>
                        <td width="175" class="TableTitle">
                            <nobr>Application:</nobr>
                        </td>
                        <td class="TableData">
                            <xsl:value-of select="BizTalkBaseObject/ApplicationName" />
                        </td>
                    </tr>
                    <tr>
                        <td width="10"></td>
                        <td width="175" class="TableTitle">
                            <nobr>Schema Type:</nobr>
                        </td>
                        <td class="TableData">
                            <xsl:value-of select="BizTalkBaseObject/SchemaType" />
                        </td>
                    </tr>
                    <tr>
                        <td width="10"></td>
                        <td class="TableTitle">
                            <nobr>Parent Assembly:</nobr>
                        </td>
                        <td class="TableData">
                            <nobr>
                                <xsl:choose>
                                    <xsl:when test="string-length(BizTalkBaseObject/ParentAssembly/Id)>0">
                                        <xsl:element name="A">
                                            <xsl:attribute name="CLASS">TableData</xsl:attribute>
                                            <xsl:attribute name="HREF">
                                                /Assembly/<xsl:value-of select="BizTalkBaseObject/ParentAssembly/Id" />.htm
                                            </xsl:attribute>
                                            <xsl:value-of select="BizTalkBaseObject/ParentAssembly/Name" />
                                        </xsl:element>
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <xsl:value-of select="BizTalkBaseObject/ParentAssembly/Name" />
                                    </xsl:otherwise>
                                </xsl:choose>
                            </nobr>
                        </td>
                    </tr>
                    <tr>
                        <td width="10"></td>
                        <td width="175" class="TableTitle">
                            <nobr>Target Namespace:</nobr>
                        </td>
                        <td class="TableData">
                            <xsl:value-of select="BizTalkBaseObject/TargetNamespace" />
                        </td>
                    </tr>
                    <tr>
                        <td width="10"></td>
                        <td width="175" class="TableTitle">
                            <nobr>Is Envelope:</nobr>
                        </td>
                        <td class="TableData">
                            <xsl:value-of select="BizTalkBaseObject/Envelope" />
                        </td>
                    </tr>
                  <!-- Added by PCA 2015-01-03 -->
                  <tr>
                    <td width="10"></td>
                    <td width="175" class="TableTitle">
                      <nobr>Description:</nobr>
                    </td>
                    <td class="TableData">
                      <xsl:value-of select="BizTalkBaseObject/CustomDescription" />
                    </td>
                  </tr>
                  <!-- End Added by PCA 2015-01-03 -->
                  <tr>
                        <td width="10"></td>
                        <td colspan="2">

                            <!-- Properties -->
                            <xsl:if test="count(BizTalkBaseObject/Properties/Property)>0">
                                <BR/>

                                <span class="PageTitle3">Schema Properties</span>

                                <ul>
                                    <xsl:for-each select="BizTalkBaseObject/Properties/Property">
                                        <li>
                                            <xsl:value-of select="text()" />
                                        </li>
                                    </xsl:for-each>
                                </ul>

                            </xsl:if>

                        </td>
                    </tr>
                    <tr>
                        <td width="10"></td>
                        <td colspan="2">

                            <!-- Imported Schema -->
                            <xsl:if test="count(BizTalkBaseObject/ImportedSchema/Schema)>0">
                                <BR/>

                                <span class="PageTitle3">Referenced Property Schema</span>

                                <ul>
                                    <xsl:for-each select="BizTalkBaseObject/ImportedSchema/Schema">
                                        <li>
                                            <xsl:element name="A">
                                                <xsl:attribute name="CLASS">TableData</xsl:attribute>
                                                <xsl:attribute name="HREF">
                                                    /Schema/<xsl:value-of select="Id" />.htm
                                                </xsl:attribute>
                                                <xsl:value-of select="Name" />
                                            </xsl:element>

                                            <UL>
                                                <xsl:apply-templates select="ImportedProperties/ImportedProperty">
                                                    <xsl:sort select="Name" />
                                                </xsl:apply-templates>
                                            </UL>
                                        </li>
                                        <br/>
                                    </xsl:for-each>
                                </ul>

                            </xsl:if>

                        </td>
                    </tr>
                    <tr>
                        <td width="10"></td>
                        <td width="175" class="TableData">
                            <BR/>
                            <BR/>
                            <xsl:element name="A">
                                <xsl:attribute name="CLASS">TableData</xsl:attribute>
                                <xsl:attribute name="HREF">
                                    /Schema/<xsl:value-of select="BizTalkBaseObject/Id" />.xml
                                </xsl:attribute>View Schema Source
                            </xsl:element>
                        </td>
                        <td class="TableData"></td>
                    </tr>
                </table>

                <BR/>
                <BR/>

                <xsl:call-template name="footer" />

            </body>
        </html>
    </xsl:template>

    <xsl:template match="ImportedProperty">
        <LI type="disc">
            <xsl:value-of select="text()" />
        </LI>
    </xsl:template>

    <xsl:template name="header">

        <table width="100%" border="0" cellpadding="0" cellspacing="0">
            <tr>
                <td width="10" background="../topBackground.jpg">
                    <nobr>
                        <img src="../topSpacer.gif"/>
                    </nobr>
                </td>
                <td width="10" background="../topBackground.jpg" valign="middle">
                    <nobr>
                        <img src="../Schema.gif" align="middle"/>
                    </nobr>
                </td>
                <td width="100%" background="../topBackground.jpg" valign="middle" CLASS="PageTitle">
                    <nobr>
                        Schema : <xsl:value-of select="./BizTalkBaseObject/Name"/>
                    </nobr>
                </td>
                <td background="../topBackground.jpg" valign="middle" align="right">
                    <IMG SRC="../topRight.jpg" ALIGN="middle"/>
                </td>
            </tr>
        </table>
        <BR/>

    </xsl:template>

    <xsl:template name="footer">
        <BR/>
        <BR/>
        <BR/>
        <BR/>
        <HR CLASS="Rule"/>
        <P CLASS="Copyright">
            Generated on: <xsl:value-of select="$GenDate"/><BR/>Microsoft.Services.Tools.BiztalkDocumenter version: <xsl:value-of select="$DocVersion"/>
        </P>
        <BR/>
    </xsl:template>


</xsl:stylesheet>
