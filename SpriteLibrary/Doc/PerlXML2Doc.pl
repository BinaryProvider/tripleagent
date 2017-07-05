#!/usr/bin/perl

# use module
use XML::Simple;
use Data::Dumper;

# create object
$xml = new XML::Simple;

# read XML file
$data = $xml->XMLin("../bin/Release/SpriteLibrary.XML");
#print Dumper($data->{members});

print "<HTML>\n";
foreach $type (P, M, E, F)
{ 
	print "<h2>Methods</h2>\n" if $type eq "M";
	print "<h2>Events</h2>\n" if $type eq "E";
	print "<h2>Properties</h2>\n" if $type eq "P";
	print "<h2>Functions</h2>\n" if $type eq "F";
	while(my ($key, $value) = each $data->{members})
	{
		#print $key;
		#while(my ($inkey, $invalue) = each $value)
		foreach $inkey (sort keys $value)
		{
			$invalue = $value->{$inkey};
			if($inkey =~ /^$type/)
			{
				my $name = $inkey;
				$name =~ s/^.:SpriteLibrary\.//;
				next if $name =~ /\.Resources\./;
				if($name =~ s/\#ctor//)
				{
					$name = "Constructor: $name";
				}
				print "<p><b>$name</b><br>\n";
				#print Dumper($invalue);
				my $summary = $invalue->{summary};
				$summary =~ s/^\s+|\s+$//g;
				print "<b>Summary:</b> $summary<br>\n" if defined $invalue->{summary};
				#if(defined $invalue->{returns})
				#{
				#	if(UNIVERSAL::isa($invalue->{returns}, 'HASH'))
				#	{
				#		#print Dumper $invalue->{returns};
				#	}
				#	else
				#	{
				#		print "Returns: $invalue->{returns}<br>\n\n" if defined $invalue->{returns};
				#	}
				#}				
				#if(defined $invalue->{param})
				#{
				#	print "<b>Parameters</b>\n";
				#	print "<ul>\n";
				#	while(my ($paramname, $description) = each $invalue->{param})
				#	{
				#		
				#		print "<li><b>$paramname</b> :";
				#		#print Dumper $description;
				#		print $description->{content};
				#		print "<br>\n";
				#	}					
				#	print "</ul>\n";
				#}
				#print "<p>\n";
			}

#			print "$inkey = \n";
#			print Dumper($invalue);G
		} 
    }
}
print "</HTML>\n";
