window.onload = function() {
    if ($("option").size() === 0) {
        $("select").prop('disabled', true);
    }
};

function SelectionChanged(selected) {
    if ($(selected).val() !== "none") {
        $("#Item1_SourceAccountName").val($("#appsettings-storage-list :selected").text().trim());
        $("#Item1_SourceAccountKey").val($(selected).val());
    } else {
        $("#Item1_SourceAccountName").val("");
        $("#Item1_SourceAccountKey").val("");
    }
}